# CUInventory

An inventory module for a chain of warehouses: purchase from suppliers, receive shipments, sell,
transfer stock between warehouses, and answer the question *"where did this actually come from?"*
for every unit sold.

Built as a layered DDD solution on ABP (.NET 10), EF Core / SQL Server, with an Angular 21 frontend
and a Server-Sent Events feed for real-time stock visibility.

This README is mostly about **why** things are the way they are. The task described business
problems rather than technical mechanisms, so the interesting part is the interpretation.

---

## Table of contents

- [How I read the domain](#how-i-read-the-domain)
- [Problem 1: Tracking the source of what they sell](#problem-1-tracking-the-source-of-what-they-sell)
- [Problem 2: Concurrent sales and stock accuracy](#problem-2-concurrent-sales-and-stock-accuracy)
- [Problem 3: Stock transfers between warehouses](#problem-3-stock-transfers-between-warehouses)
- [Problem 4: Real-time visibility](#problem-4-real-time-visibility)
- [Problem 5: Reporting](#problem-5-reporting)
- [Indexes and why each one exists](#indexes-and-why-each-one-exists)
- [Architecture](#architecture)
- [Cross-cutting concerns](#cross-cutting-concerns)
- [Assumptions](#assumptions)
- [Running it locally](#running-it-locally)
- [Tests](#tests)

---

## How I read the domain

A few translations from the brief into the model:

**"The client" is a tenant.** The brief describes one client with several branches, but "a chain of
warehouses" run by a business is exactly the shape that multi-tenancy fits. Every aggregate that
holds business data implements `IMultiTenant`, so a second client can be onboarded without a schema
change. This costs almost nothing today and saves a migration later.

**"Sales staff" are users.** The concurrency problem is about two humans hitting the same row at the
same time, which is really authentication and authorization, and ABP already provides both. No
separate staff entity was needed.

**Customers are not modeled.** The brief never asks anything about the buyer: no credit, no returns,
no customer history. A `Customer` entity would be a field on `Sale` that nothing reads. I left it out
rather than add a table that only looks like domain modeling.

**A purchase order and a shipment are two different things.** This is the deliberate one. The client
buys from suppliers, and it would have been simpler to treat "we bought it" and "it arrived" as one
event. Real procurement doesn't work that way. You order 500 units and they arrive as three shipments
over six weeks, sometimes short, sometimes late. So:

```
PurchaseOrder  (what we agreed to buy: supplier, lines, outstanding quantities)
     │  1..*
Shipment       (what physically arrived, dispatched then received at a warehouse)
     │  1..*  on receipt
InventoryLot   (a distinct parcel of stock, frozen with its supplier, cost, and arrival date)
```

This split adds complexity: partial receipts, outstanding quantities, a `PartiallyReceived` order
status. But it's what makes Problem 1 answerable at all. Without it, "what's left from the March
shipment?" has nowhere to be answered from. It also produces the more interesting scenarios, like the
same product, from the same supplier, at two different costs, sitting in two different warehouses.

The full aggregate list: `Category`, `Product`, `Supplier`, `Warehouse`, `PurchaseOrder`, `Shipment`,
`InventoryLot`, `InventoryBalance`, `StockTransfer`, `Sale`.

---

## Problem 1: Tracking the source of what they sell

> *Which purchase did this sold quantity come from? Which supplier? How much of Product X sold that
> came from Supplier Y? What's left from the March shipment?*

The client described a need, not a mechanism. The mechanism I chose is **lot-based inventory with
persisted allocations**.

### Lots

When a shipment is received, each shipment line becomes an `InventoryLot`
(`ShipmentManager.ReceiveAsync`). A lot is a parcel of stock that remembers where it came from:

| Field | Why it's there |
|---|---|
| `SupplierId` | "How much of Product X came from Supplier Y?" |
| `ShipmentLineId` | Traces back to the shipment, and through it to the purchase order |
| `ReceivedAt` | "What's left from the shipment we received last March?" |
| `UnitCost` | Cost is a property of *that* purchase, not of the product. This is what makes COGS and margin real |
| `OriginalQuantity` / `RemainingQuantity` / `ReservedQuantity` | Original never changes, so a lot stays a permanent historical record even after it's fully consumed |
| `Source` | `Purchase` or `TransferIn`: where the parcel entered this warehouse from |

A lot is **immutable in its identity** and only moves quantity. That's the whole trick. History stays
queryable because nothing overwrites it.

### Allocation

A sale doesn't just decrement a number. It decides *which lots* it consumes, and that decision is a
strategy (`Domain/Inventory/Allocation/`):

- **`FifoAllocationStrategy`** is the default. Oldest `ReceivedAt` first, tie-broken by `Id` so the
  order is deterministic. That last part matters, because the concurrency test depends on two racers
  picking the same lot.
- **`SpecificSupplierAllocationStrategy`** for "sell me the Supplier Y stock specifically."
- **`SpecificLotAllocationStrategy`** to sell from one exact parcel.
- **`SpecificWarehouseAllocationStrategy`** to constrain to one warehouse.

They share `InventoryAllocationStrategyBase`, which does the fill-until-satisfied walk, and each
subclass only supplies `SelectCandidates`. `InventoryAllocationService` picks the strategy by
`AllocationStrategyKind` off the request.

This is the Strategy pattern where it earns its keep. The selection rule is exactly the thing the
client will want to change (LIFO, FEFO for perishables, cheapest-first), and it's the thing that
varies per sale line. Adding LIFO is a new class and an enum value, with no existing code touched.
That's Open/Closed applied where change is actually likely, not sprinkled everywhere.

### The permanent link

Every allocation is persisted as a `SaleAllocation` row hanging off the sale line:

```
Sale → SaleLine → SaleAllocation { InventoryLotId, SupplierId, WarehouseId, UnitCost, Quantity }
```

The supplier and unit cost are **denormalized onto the allocation**, not just reachable through the
lot. That's intentional. The allocation is a historical fact about what was sold, so it shouldn't
change if someone later edits the lot, and reports shouldn't need a join to answer the most common
question.

So both example reports fall out directly:

- *"How much of Product X came from Supplier Y?"* groups `SaleAllocation` by `SupplierId`, `ProductId`.
- *"What's left from the March shipment?"* reads `InventoryLot.RemainingQuantity` where `ReceivedAt`
  is in March.

The lineage survives warehouse transfers too. See Problem 3.

---

## Problem 2: Concurrent sales and stock accuracy

> *Two staff sold the same item at once and stock went negative. Customers were promised things that
> weren't there.*

Those are two different bugs, and they need two different fixes.

### "Promised things that weren't there" means reserve, then confirm

A sale has a lifecycle: `Draft → Confirmed`, or `Draft → Cancelled`.

- **Create** (`SaleManager.CreateAsync`) allocates lots and **reserves** the quantity. Nothing is
  deducted yet. `QuantityReserved` goes up, `QuantityOnHand` doesn't move.
- **Confirm** (`ConfirmAsync`) converts reservation to consumption: reserved down, on-hand down.
- **Cancel** (`CancelAsync`) releases the reservation and puts availability straight back.

`InventoryBalance` exposes `QuantityAvailable = QuantityOnHand - QuantityReserved`, and every
availability check reads *available*, never on-hand. So the moment a staff member drafts a sale, those
units stop being sellable to anyone else. You cannot promise stock that another draft already holds.
The reservation is the promise, made durable.

### "Stock went negative" needs two layers, not one

**Layer 1: the domain invariant.** `InventoryBalance.Reserve` and `DeductDirect` throw
`InsufficientStockDomainException` if `QuantityAvailable < quantity`. `InventoryLot.Reserve` and
`Consume` guard the same way per parcel. Stock can never go negative *in memory*.

**Layer 2: optimistic concurrency at the database.** Layer 1 alone doesn't save you. Two requests both
read `available = 10`, both pass the check, both write. Classic lost update. So `InventoryBalance`
carries a `ConcurrencyStamp`, and the second writer's `UPDATE` matches zero rows because the stamp
moved, so EF Core throws `AbpDbConcurrencyException`. The whole unit of work rolls back, including the
sale row. The loser gets a clean failure rather than a corrupted balance.

`InventoryBalance` is deliberately a **small, hot aggregate**: one row per (warehouse, product),
holding just the two quantities and a threshold. This is what makes optimistic concurrency practical,
because the contended row is tiny, so the window between read and write is short and genuine
collisions are rare. Had I hung the balance off `Warehouse`, every sale in a warehouse would contend
with every other sale in that warehouse. Aggregate boundaries here are a concurrency decision, not a
modeling aesthetic.

**Why optimistic over pessimistic locking.** `SELECT ... WITH (UPDLOCK)` would also work, and under
extreme contention on one SKU it would actually be more efficient. I chose optimistic because:

- No held locks means no lock convoys and no deadlock risk across the multi-row writes a sale makes
  (sale, lines, allocations, balances, lots).
- Contention is genuinely rare in this domain. Two staff hitting the *same product in the same
  warehouse* within the same few milliseconds is the exception, and the brief describes it as one.
- The failure is explicit and safe. The loser sees a rejection and retries. Nothing is silently wrong.

The trade-off is honest. Under a genuine thundering herd on one SKU, like a flash sale, optimistic
concurrency degrades into retry storms and pessimistic locking would win. The retry is currently the
caller's job, since the app service doesn't loop. That's a deliberate limit rather than an oversight.
An automatic server-side retry would silently re-price a sale against different lots than the ones the
user saw, so it belongs in the UI where the user can be told.

**Belt and braces:** there are `CHECK` constraints in the database too (`QuantityOnHand >= 0`,
`QuantityReserved <= QuantityOnHand`, and the equivalents on lots). If a bug ever gets past both
layers, the database refuses the write rather than storing a nonsense row.

### The test

`test/CUInventory.EntityFrameworkCore.Tests/.../InventoryConcurrencyTests.cs`

`Two_Concurrent_Reservations_For_The_Same_Limited_Stock_Only_One_Succeeds` seeds 10 units and stages
the race deterministically, using two units of work and two DI scopes. The "loser" reads the balance
first (stamp S) and holds its snapshot. The "winner" then reads, reserves all 10, and commits. The
loser reserves against its now-stale snapshot and tries to commit.

It asserts three things, because all three are what the client actually cares about:

1. The loser's commit throws `AbpDbConcurrencyException`. Rejected, not oversold.
2. Exactly **one** sale exists. The loser's sale insert rolled back with its failed balance update.
3. Final state is `OnHand 10, Reserved 10, Available 0`. No negative stock, nothing lost.

I staged it deterministically rather than firing two `Task.Run`s at it. A real race is
non-deterministic by definition and gives you a test that passes for the wrong reason on a fast
machine. This one interleaves the exact operations that cause the bug, every run.

---

## Problem 3: Stock transfers between warehouses

> *Source says "sent," destination never recorded "received." What if it fails midway? What if the
> network drops?*

The client's instinct is that a transfer is one action. That's the bug. Physically, a transfer is
*not* atomic, because goods spend real time on a truck. Any design that pretends otherwise has to lie
about where the stock is during that window.

So the model makes the truck a first-class state:

```
Draft ──dispatch──> Dispatched ──receive──> Received
  │                      │
  └───cancel─────────────┴──cancel──> Cancelled  (restores source stock)
```

- **Dispatch** (`StockTransferManager.DispatchAsync`) FIFO-allocates from the source warehouse, calls
  `Consume` on the source lots, deducts the source `InventoryBalance`, and records a
  `TransferAllocation` per consumed lot. Stock leaves the source. It does **not** appear at the
  destination.
- **Receive** (`ReceiveAsync`) creates a **new `InventoryLot`** at the destination for each
  `TransferAllocation`, and increases the destination balance.

### Why this answers every failure question

**"What if it fails midway?"** Each transition is a single atomic transaction. Dispatch either fully
happens or fully doesn't. There is no half-dispatch.

**"What if the network drops between steps?"** Then the transfer sits in `Dispatched`. That's not a
broken state, it's an **accurate** one. The goods are genuinely on a truck: gone from the source, not
yet at the destination. The system is telling the truth. Nothing is lost, because the
`TransferAllocation` rows are a precise manifest of what's in transit, down to the source lot.

**"How do you ensure both warehouses end up consistent?"** By never requiring them to change together.
The old failure mode, where the source says sent and the destination never received, is impossible
here because the destination isn't *supposed* to have it yet. The stock is always in exactly one of
three accounted places: source, in transit, or destination. `StockTransfer.Status` is indexed
precisely so you can ask "what's been in transit too long?" and chase it.

The alternative, one transaction spanning both warehouses, would look tidier and would be a lie. It
would mean stock teleports, and it would fall apart the moment warehouses live in separate databases.
This shape doesn't.

**Cancel** is the compensating action. If the transfer was already dispatched, `CancelAsync` restores
the source lots via `InventoryLot.Restore`, which refuses to exceed `OriginalQuantity`, and adds the
quantity back to the source balance. Receiving a cancelled transfer is impossible, because
`MarkReceived` only accepts `Dispatched`, and a received transfer can't be cancelled.

### Lineage survives the move

This is the part that ties back to Problem 1. The destination lot is **not** a fresh anonymous parcel.
It's created carrying the original `SupplierId`, the original `UnitCost`, the original `ReceivedAt`,
and the original `ShipmentLineId`. Only `Source` becomes `TransferIn`.

So moving stock from Warehouse A to Warehouse B doesn't launder its origin. Sell it in B six months
later and the report still says: Supplier Y, March shipment, that cost. Transfers preserve FIFO
ordering too, because the copied `ReceivedAt` is the *original* arrival date rather than the transfer
date. Otherwise every transfer would quietly make old stock look new and corrupt FIFO everywhere.

---

## Problem 4: Real-time visibility

> *Managers want to see stock changes as they happen, and be notified when a product falls below a
> configurable threshold.*

The brief asks for SignalR. **I used Server-Sent Events instead.** That's a deliberate deviation, so
here's the argument.

### Why SSE over SignalR

The requirement is strictly one-way: the server pushes, the client listens. Clients never send
anything back over this channel. SSE is exactly that shape, and since .NET 10 it's first-class in the
framework (`TypedResults.ServerSentEvents`, `SseItem<T>`), so there's no extra package, no hub, no
negotiation handshake, and no fallback transports.

SignalR earns its complexity when you need bidirectional messaging, groups, or transport fallback. Its
main historical advantage, WebSocket fallback, matters far less now that SSE is supported everywhere
the app targets. None of that is in scope here. SSE is plain HTTP, so it rides the existing bearer-auth
pipeline and passes through proxies unchanged.

Its real limitation is that the browser's native `EventSource` can't send an `Authorization` header,
which is handled on the client (below). The roughly six-connections-per-origin cap on HTTP/1.1 is a
non-issue over HTTP/2. If bidirectional needs appear later, SignalR goes behind the same
`IStockNotificationBroadcaster` interface without touching the domain.

### How it works

```
InventoryBalance mutates (reserve / release / confirm / receive / deduct)
  → raises StockChangedDomainEvent, and LowStockReachedDomainEvent on a downward
    threshold crossing                                      [Domain, ABP local events]
  → StockNotificationEventHandler                           [Application]
       builds a StockNotificationDto and defers the publish to
       UnitOfWork.OnCompleted (so a rolled-back change never notifies)
  → IStockNotificationBroadcaster.Publish                   [Application.Contracts]
  → ChannelStockNotificationBroadcaster (singleton)         [Application]
       fans out to one bounded Channel<T> per connected subscriber
  → GET /api/inventory/stock-notifications/stream           [HttpApi]
       streams to the client as SSE
```

Both feeds share one connection, tagged by SSE `event` type (`StockChanged`, `LowStockReached`), so a
client renders a live activity feed and distinct low-stock alerts from a single stream.

### Design decisions

**Publish only after commit.** ABP raises local domain events during `SaveChangesAsync`, still inside
the transaction. The handler snapshots the notification immediately but wraps the actual `Publish` in
`IUnitOfWorkManager.Current.OnCompleted(...)`. A manager is never told about a change that then rolls
back. This matters a lot here, because Problem 2's whole design *expects* losing writers to roll back.
Without it, every rejected oversell would fire a phantom alert. Covered by
`A_Rejected_Oversell_Publishes_No_Notification`.

**Bounded, drop-oldest channels per subscriber.** `StockChanged` is high-frequency, and a stalled
client must never block publishers or leak memory. Each subscriber gets a bounded channel with
`DropOldest`. The trade-off is that a slow client can miss intermediate events. That's acceptable
because the database remains authoritative and the low-stock alert still fires. Availability of the
system beats completeness of a live feed.

**Alerting is an edge, not a level.** `LowStockRule` fires `LowStockReachedDomainEvent` only when
availability was *above* the threshold before the operation and is *below* it after. Otherwise every
subsequent sale of an already-low product would re-alert, and managers would learn to ignore the
alerts. The threshold is per (warehouse, product) and nullable, where `null` means "don't watch this",
and it's set via `InventoryBalanceAppService.SetLowStockThresholdAsync`.

**In-memory fan-out.** The broadcaster is a process-local singleton, which is correct for the
single-host deployment here. Scaling out means swapping it for a Redis pub/sub or ABP distributed event
bus backplane behind the same interface. Domain, handler, and endpoint don't change.

**Tenant isolation.** Every notification carries its `TenantId`, and the endpoint filters to the
current tenant.

### Consuming it

The stream is gated by `CUInventory.InventoryBalances.SubscribeNotifications` and requires a bearer
token. Since native `EventSource` can't attach headers, the Angular client uses a hand-written `fetch`
plus `ReadableStream` reader that carries the token and feeds a signal store, driving app-wide toasts
for low stock and live tiles on the dashboard.

Quick manual check:

```bash
curl -N -H "Authorization: Bearer <token>" \
  http://localhost:8080/api/inventory/stock-notifications/stream
```

---

## Problem 5: Reporting

> *Filter by warehouse, supplier, category, date range. Must perform well as the database grows.*

Reporting is a fundamentally different job from transactional work. It reads wide and aggregates,
where the rest of the app reads narrow and mutates. Forcing it through the aggregate repositories
would mean loading `Sale` graphs into memory to sum them, which is fine at 1,000 rows and fatal at
10 million.

So reporting gets its own read slice: **`IReportingRepository`**, declared in `Domain/Reporting` and
implemented by `EfCoreReportingRepository`. It returns flat read models, never aggregates. It's a
read-side CQRS split without the ceremony of full CQRS: no separate store, no projections to keep in
sync, just an honest acknowledgement that reads and writes have different shapes.

**Endpoints** (`ReportsAppService`), all taking the same `ReportFilterInput` with warehouse, supplier,
category, product, from/to date, plus paging and sorting:

| Endpoint | Answers |
|---|---|
| `GetSalesBySource` | *"How much of Product X did we sell from Supplier Y?"*, grouped, with revenue and cost |
| `GetSalesBySourceDetail` | The same, drilled down to individual sale lines |
| `GetRemainingStock` | *"What's left from the shipment we got last March?"*, by lot |
| `GetRemainingStockDetail` | Lot-level breakdown |
| `GetInventoryValuation` | Stock on hand valued at real per-lot purchase cost |
| `GetLowStock` | Everything currently under its threshold |

**What keeps it fast as data grows:**

- **Everything aggregates server-side.** `GroupBy`, `Sum`, and `LongCount` translate to SQL. No entity
  is ever materialized to be summed in C#.
- **Everything is paged.** `SkipCount` and `MaxResultCount` are applied *before* the `Select`, so SQL
  Server does `OFFSET/FETCH`. There is no unbounded endpoint.
- **Every filter maps to an index.** See the next section. The filter set in the brief drove which
  indexes exist, not the other way round.
- **Filters compose predicate by predicate**, so an unfiltered report and a fully-filtered one are the
  same query shape with fewer `WHERE` clauses.
- **`IIsActive` is disabled inside report queries** via `dataFilter.Disable<IIsActive>()`. History must
  stay truthful, so deactivating a product today must not retroactively erase last year's sales of it.

**Known EF gotcha, documented for the discussion:** projections feeding a `GroupBy` use member-init
rather than positional-record syntax, because EF Core can't translate positional record construction
into a groupable expression.

**Trade-off:** these read straight from the transactional tables. That's the right call at this scale
and keeps the numbers exact and live. If reports eventually outgrow it, the next step is a materialized
summary table (or indexed views) refreshed off the existing domain events, and the
`IReportingRepository` seam means that swap doesn't touch the application layer.

---

## Indexes and why each one exists

Every index below exists because a specific query in this codebase needs it. I didn't index for
hypothetical queries. Each one pays for itself in write cost, so it has to earn its place.

### Inventory (the hot path)

| Index | Why |
|---|---|
| `InventoryBalance (WarehouseId, ProductId)` **UNIQUE** | Two jobs. It's the lookup for *every* stock operation, since sale, transfer, and receipt all start by fetching this row. And being unique, it's the guard that stops concurrent get-or-create from ever producing two balance rows for one (warehouse, product), which would silently split the truth in half and defeat the concurrency control. |
| `InventoryLot (ProductId, WarehouseId, ReceivedAt, Id)` | The FIFO allocation query, exactly. Filter on product and warehouse, order by `ReceivedAt` then `Id`. Column order matches the query, with equality predicates first and then the sort. SQL Server walks it in order and stops as soon as the quantity is filled, so there's no sort and no scan. This is the single most important index in the system, and it's on the path of every sale. |
| `InventoryLot (SupplierId)` | *"How much stock do we still hold from Supplier Y?"* The remaining-stock and valuation reports filter here. |
| `InventoryLot (ReceivedAt)` | *"What's left from the March shipment?"* Date-range filtering when the report isn't scoped to one product. |
| `InventoryLot (ShipmentLineId)` | Traces a lot back to its shipment, and through it to the purchase order. Also the join for receipt-time reconciliation. |

### Source tracking (Problem 1's reports)

| Index | Why |
|---|---|
| `SaleAllocation (SupplierId)` | The literal index for *"how much of Product X did we sell that came from Supplier Y?"* This is the flagship report and it filters here first. |
| `SaleAllocation (InventoryLotId)` | The reverse trace: given a lot, which sales consumed it? Needed for lot-level drill-down, and for confirming or cancelling a sale's reservations. |
| `Sale (Status, ConfirmedAt)` | Composite, and the order matters. Every sales report wants confirmed sales in a date range. `Status` is the equality predicate, so it leads, and `ConfirmedAt` handles the range and the sort. Reversing them would force a scan of every status. |
| `SaleLine (ProductId)` | Product filtering on sales reports, and the join from product to sales history. |

### Workflow status (operational queries)

| Index | Why |
|---|---|
| `StockTransfer (Status)` | *"What's still in transit?"* Directly supports the Problem 3 recovery story: find transfers stuck in `Dispatched` after a crash or an abandoned truck. |
| `Shipment (Status)` | The receiving desk's worklist: what's dispatched and awaiting receipt. |
| `Shipment (PurchaseOrderId)` | *"Which shipments belong to this order?"* Drives outstanding-quantity calculation on partial receipts. |
| `PurchaseOrder (Status)` | Open-orders list, and the guard when creating a shipment, since only `Confirmed` and `PartiallyReceived` orders can ship. |
| `PurchaseOrder (SupplierId)` | Supplier-scoped procurement history. |
| `TransferAllocation (SourceLotId)` | Lineage: which source lot did this in-transit quantity come from? Used on receive to copy supplier, cost, and date, and on cancel to restore the exact lot. |
| `ShipmentLine (ProductId)`, `PurchaseOrderLine (ProductId)` | Product-scoped procurement filtering, and join support from the product side. |

### Natural keys (uniqueness, not just speed)

| Index | Why |
|---|---|
| `Product.Sku` **UNIQUE**, filtered `WHERE Sku IS NOT NULL` | SKU is the business identity of a product, and duplicates are a data-quality incident. Filtered because SKU is optional, and SQL Server otherwise treats multiple `NULL`s as duplicates and blocks legitimate rows. |
| `Warehouse.Code` **UNIQUE** | Codes are how staff refer to warehouses out loud. Two `WH-01`s is a support ticket. |
| `Category.Name` **UNIQUE** | Prevents the "Electronics" versus "electronics" drift that quietly splits reports. |
| `Supplier.ContactInfo.Email` **UNIQUE**, `Supplier.ContactInfo.PhoneNumber` **UNIQUE** | Catches duplicate supplier records at entry. A duplicated supplier is corrosive here specifically, because it fragments exactly the source-tracking reports this whole system exists to produce. |

**What I chose *not* to index:** no covering indexes with `INCLUDE` columns yet, and no index on
`IsDeleted`. Both are premature. The first needs real query plans from real data volumes to size
correctly, and the second is low-selectivity, since nearly every row is `false`, so SQL Server would
usually ignore it while I'd pay the write cost on every insert.

---

## Architecture

Clean Architecture, realized as an ABP layered solution. Dependencies point inward, so the domain knows
nothing about EF Core, HTTP, or Angular.

```
src/
  CUInventory.Domain.Shared         Enums, error codes, localization. No behavior
  CUInventory.Domain                Aggregates, domain services, repository interfaces  ← the core
  CUInventory.Application.Contracts DTOs, app service interfaces, permissions
  CUInventory.Application           Use-case orchestration, event handlers, broadcaster
  CUInventory.EntityFrameworkCore   DbContext, configurations, repository implementations
  CUInventory.HttpApi               Controllers (the SSE endpoint lives here)
  CUInventory.HttpApi.Host          Host, Swagger, auth
  CUInventory.DbMigrator            Migrations + seed data
  CUInventory.AppHost               .NET Aspire orchestration
angular/                            Angular 21 frontend
```

Mapping to the brief's Domain / Application / Infrastructure / API: `Domain` and `Domain.Shared` are
the domain, `Application` and `Application.Contracts` are application, `EntityFrameworkCore` is
infrastructure, and `HttpApi` plus `HttpApi.Host` are the API.

**Where the business logic lives.** All of it is in `Domain`. `InventoryBalance` won't let itself go
negative. `Sale` won't confirm twice. `StockTransfer` won't receive what wasn't dispatched. Domain
managers (`SaleManager`, `StockTransferManager`, `ShipmentManager`, `InventoryBalanceManager`) hold the
logic that spans aggregates, because allocating a sale across lots and balances isn't any single
aggregate's job.

Application services **only orchestrate**: authorize, load, delegate, persist, map. There are no
pass-through managers, since a manager exists only where there is real cross-aggregate logic to hold.
And the domain layer has no `DbContext` reference anywhere. Managers take already-loaded aggregates as
parameters and return them, which is what makes `SaleManagerTests` and `StockTransferManagerTests` pure
unit tests with no database.

**On SOLID:** applied where change is likely, not as decoration. The allocation strategies are the
clearest case, combining Open/Closed with Strategy, because the rule *will* change.
`IReportingRepository` is Interface Segregation, since reporting needs a completely different contract
from CRUD and gets one rather than bloating the aggregate repositories. `IClock` over `DateTime.UtcNow`
throughout keeps time injectable and the tests deterministic.

**Notable patterns:** Aggregate, Repository, Domain Service, and Domain Events (DDD); Strategy
(allocation); Value Objects (`Quantity`, `Money`, `Sku`, `Address`, `ContactInfo`, so a `Quantity` can't
be negative and you can't pass a price where a count belongs); Unit of Work (transactional boundaries,
ABP-provided); and Publish/Subscribe (real-time fan-out).

---

## Cross-cutting concerns

**Audit fields.** Every business aggregate derives from `FullAuditedAggregateRoot<Guid>`, so
`CreatorId`, `CreationTime`, `LastModifierId`, `LastModificationTime`, `DeleterId`, and `DeletionTime`
are populated automatically. The brief asked for `CreatedBy/CreatedAt/ModifiedBy/ModifiedAt`, and these
are the same fields under ABP's naming, filled by interceptor rather than by hand at each call site.

**Soft delete.** `FullAudited*` implies `ISoftDelete`. Deletes set `IsDeleted` and a global query
filter hides the rows. In an inventory system this is not optional, because hard-deleting a supplier
would orphan the allocation history that Problem 1 depends on.

**A separate `IIsActive` convention.** Distinct from soft delete, and worth calling out. Products,
categories, and warehouses can be *deactivated*: still real, just not offered for new work. The filter
is **disabled by default** and enabled only in `CrudAppService.GetListAsync`, so a deactivated product
still resolves everywhere it's referenced by history. Reports explicitly disable it, as above, so
deactivating a product never rewrites the past.

**Error handling.** Domain exceptions are data-only classes extending ABP's `BusinessException`, each
carrying a stable error code from `CUInventoryDomainErrorCodes` plus the data that makes the message
actionable: requested quantity, available quantity, the relevant ids. They carry no behavior. The throw
site decides the situation, and the exception just describes it.

The payoff is that a business rule failure is never an accident. `InsufficientStockDomainException`
reaches the client as a structured error with a stable code, a localized message from `en.json`, and
the numbers attached, while a genuine bug still surfaces as a `500`. ABP's exception middleware does the
mapping via its default status-code finder: validation failures to `400`, missing entities to `404`,
authorization to `401` or `403`, and `IBusinessException` to ABP's business-error default. I haven't
overridden that mapping, since `AbpExceptionHttpStatusCodeOptions` is untouched. That's a deliberate
"framework default until there's a reason" call.

**Concurrency stamps over the wire.** Mutating endpoints take the client's `ConcurrencyStamp` in the
request DTO (`IHasConcurrencyStamp`), so stale-read conflicts surface as a rejection rather than a
silent last-write-wins.

**Authorization.** Every operation is permission-gated via `CUInventoryPermissions`, including
per-action grants like `Sales.Confirm`, `StockTransfers.Dispatch`, `Shipments.Receive`,
`InventoryBalances.SetThreshold`, and `InventoryBalances.SubscribeNotifications`.

**Swagger** is available at the API root and covers every endpoint.

---

## Assumptions

Documented because the brief invited them.

1. **Staff can sell from any warehouse, or several at once.** The brief doesn't tie staff to branches.
   A sale line may target a specific warehouse or leave it open and let allocation choose, so one sale
   can draw from multiple warehouses.
2. **One purchase order, many shipments.** The core modeling decision, explained up top. A PO can be
   `PartiallyReceived`, and a shipment can't exceed the outstanding quantity on its order line.
3. **FIFO is the default consumption rule.** Sensible for general merchandise, and it matches
   accounting convention. Not hardcoded: it's one strategy among several, and a sale can override per
   line.
4. **Costing is real per-lot cost, not average.** Each lot keeps its own purchase price, so valuation
   and margin are exact rather than smoothed. This is the direct payoff of lot tracking.
5. **A customer isn't part of this module.** Explained above.
6. **Thresholds are per (warehouse, product), and nullable.** "Low" means something different in the
   central warehouse than in a small branch. `null` means not monitored.
7. **Alerts fire on the downward crossing only**, not on every operation below the line.
8. **Single-host real-time.** In-memory fan-out, so multi-host needs a backplane behind the existing
   interface.
9. **Concurrency retries belong to the caller.** The server rejects cleanly, and the UI decides whether
   to re-price and retry.
10. **Prices and costs are single-currency.** `Money` wraps a decimal without a currency code. Adding
    one is a `Money` change, not a schema-wide one.
11. **Transfers move goods, not ownership.** No inter-branch billing.
12. **Multi-tenant from day one**, even though the brief describes one client.

---

## Running it locally

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet)
- [Node.js 18 or 20](https://nodejs.org/en)
- Docker Desktop, for SQL Server, or point at your own instance

### Option A: .NET Aspire (recommended)

One command brings up SQL Server, runs migrations and seed data, starts the API, and starts Angular:

```bash
cd src/CUInventory.AppHost
dotnet run
```

The Aspire dashboard opens with links to everything. SQL Server runs in a container with a persistent
volume, the `Migrator` runs to completion before the API starts, and the API is healthy before the
frontend comes up. CloudBeaver, a database UI, is included at `:8978` if you want to poke at the tables
directly.

Aspire will prompt for the `SqlPassword` and `CloudBeaverAdminPassword` parameters on first run and
store them in user secrets.

To use your own SQL Server instead of the container, set `UseSqlContainer=false` and provide the
`Default` connection string.

### Option B: Docker Compose

A compose file generated from the Aspire AppHost lives at
`src/CUInventory.AppHost/aspire-output/docker-compose.yaml`. See
[`DOCKER_COMPOSE_README.md`](./DOCKER_COMPOSE_README.md) for details.

```bash
cd src/CUInventory.AppHost/aspire-output
docker compose up -d --build
```

- Frontend: http://localhost:4200
- API and Swagger: http://localhost:8080
- CloudBeaver: http://localhost:8978

### Option C: run the pieces yourself

```bash
# 1. Client-side libs (only needed if you cloned rather than generated)
abp install-libs

# 2. Create the database, apply migrations, seed data
dotnet run --project src/CUInventory.DbMigrator

# 3. API
dotnet run --project src/CUInventory.HttpApi.Host

# 4. Frontend
cd angular && npm install && npm start
```

Check the `Default` connection string in `appsettings.json` under `CUInventory.HttpApi.Host` and
`CUInventory.DbMigrator` first.

### Seed data

`DbMigrator` seeds a working dataset rather than empty tables: categories, products, suppliers,
warehouses, and then a full **realistic flow**. Purchase orders get confirmed, shipments get received
(creating lots with staggered `ReceivedAt` dates and different costs), stock gets transferred between
warehouses, and sales get made. So the reports have something real to say the moment you log in, and
FIFO across differently-dated lots is visible immediately.

Sign in with the standard ABP admin account (`admin` / `1q2w3E*`).

### Production signing certificate

For a production deployment ABP expects an `openiddict.pfx`:

```bash
dotnet dev-certs https -v -ep openiddict.pfx -p <your-password>
```

See [Configuring OpenIddict](https://abp.io/docs/latest/Deployment/Configuring-OpenIddict#production-environment).

---

## Tests

Roughly 228 test methods across three projects:

```bash
dotnet test
```

| Project | Covers |
|---|---|
| `CUInventory.Domain.Tests` | Pure unit tests on the core logic: aggregate invariants, allocation strategies, sale and transfer managers. No database. |
| `CUInventory.Application.Tests` | Use-case orchestration, authorization, the SSE broadcaster and event handler. |
| `CUInventory.EntityFrameworkCore.Tests` | Integration tests against the real EF Core stack: concurrency, reporting queries, notification-after-commit semantics. |

The domain tests are fast and database-free precisely because of the layering choice above. Managers
take already-loaded aggregates and return them, so testing the allocation and sale logic needs no
`DbContext` and no mocking framework.

Several suites are written as **abstract generic bases** parameterized by startup module.
`StockNotificationTests<TStartupModule>` in `Application.Tests` is the example, and
`EfCoreStockNotificationTests` inherits it to run the identical assertions against the real EF Core
stack. The same behavioral contract is verified twice: once fast, once for real. Worth a look, since
it's how the notification semantics are pinned at both levels.

Two tests worth opening first:

- **`InventoryConcurrencyTests.Two_Concurrent_Reservations_For_The_Same_Limited_Stock_Only_One_Succeeds`**
  is the required concurrency test, detailed under Problem 2.
- **`StockNotificationTests.A_Rejected_Oversell_Publishes_No_Notification`** proves the two designs
  compose: a rejected concurrent sale rolls back *and* stays silent, with no phantom alert. It runs
  against both the application stack and EF Core.
