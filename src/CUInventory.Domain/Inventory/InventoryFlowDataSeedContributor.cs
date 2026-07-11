using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Catalog.Aggregates;
using CUInventory.Catalog.Repositories;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Interfaces;
using CUInventory.Inventory.Repositories;
using CUInventory.Procurement;
using CUInventory.Procurement.Aggregates;
using CUInventory.Procurement.Repositories;
using CUInventory.Sales;
using CUInventory.Sales.Interfaces;
using CUInventory.Sales.Repositories;
using CUInventory.ValueObjects;
using CUInventory.Warehousing;
using CUInventory.Warehousing.Aggregates;
using CUInventory.Warehousing.Interfaces;
using CUInventory.Warehousing.Repositories;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace CUInventory.Inventory;

/// <summary>
/// Seeds the six interdependent aggregates (PurchaseOrder, Shipment, InventoryLot,
/// InventoryBalance, StockTransfer, Sale) by running the real domain flow through the Managers,
/// so the resulting stock levels stay consistent. Written as a single contributor so the whole
/// saga executes in one deterministic sequence, and guarded so it is idempotent and safe under
/// any ABP data-seed-contributor ordering.
/// </summary>
public class InventoryFlowDataSeedContributor(
    IClock clock,
    IProductRepository productRepository,
    ISupplierRepository supplierRepository,
    IWarehouseRepository warehouseRepository,
    IInventoryBalanceManager inventoryBalanceManager,
    IShipmentManager shipmentManager,
    IStockTransferManager stockTransferManager,
    ISaleManager saleManager,
    IPurchaseOrderRepository purchaseOrderRepository,
    IShipmentRepository shipmentRepository,
    IInventoryLotRepository inventoryLotRepository,
    IStockTransferRepository stockTransferRepository,
    ISaleRepository saleRepository)
    : IDataSeedContributor, ITransientDependency
{
    private const decimal PurchasedQuantity = 100m;
    private const decimal TransferredQuantity = 20m;
    private const decimal SoldQuantity = 10m;
    private const decimal UnitCost = 10m;
    private const decimal UnitPrice = 15m;

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        if (await saleRepository.GetCountAsync() > 0)
        {
            return;
        }


        var warehouses = await warehouseRepository.GetListAsync();
        var products = (await productRepository.GetListAsync())
            .Where(p => !p.IsService)
            .Take(3)
            .ToList();
        var suppliers = await supplierRepository.GetListAsync();

        if (warehouses.Count < 2 || products.Count == 0 || suppliers.Count == 0)
        {
            Console.WriteLine("InventoryFlowDataSeedContributor: prerequisites missing (warehouses/products/suppliers). Skipping.");
            return;
        }

        var mainWarehouse = warehouses[0];
        var secondWarehouse = warehouses[1];
        var supplier = suppliers[0];
        var now = clock.Now;

        try
        {
            // 1. Purchase order for the selected products, confirmed and ready to receive.
            var purchaseOrder = new PurchaseOrder(
                Guid.NewGuid(),
                supplier.Id,
                mainWarehouse.Id,
                products
                    .Select(p => new PurchaseOrderLineData(
                        Guid.NewGuid(), p.Id, new Quantity(PurchasedQuantity), new Money(UnitCost)))
                    .ToList());
            purchaseOrder.Confirm(now);
            await purchaseOrderRepository.InsertAsync(purchaseOrder);

            // 2. Inventory balances at the destination warehouse.
            var mainBalances = new List<InventoryBalance>();
            foreach (var product in products)
            {
                mainBalances.Add(await inventoryBalanceManager.GetOrCreateAsync(mainWarehouse.Id, product.Id));
            }

            // 3. Shipment against the purchase order -> creates lots, raises balances, registers receipt.
            var shipment = new Shipment(
                Guid.NewGuid(),
                purchaseOrder.Id,
                supplier.Id,
                mainWarehouse.Id,
                products
                    .Select(p => new ShipmentLineData(
                        Guid.NewGuid(), p.Id, new Quantity(PurchasedQuantity), new Money(UnitCost)))
                    .ToList());
            shipment.Dispatch(now);
            var lots = await shipmentManager.ReceiveAsync(shipment, purchaseOrder, mainBalances);

            await shipmentRepository.InsertAsync(shipment);
            foreach (var lot in lots)
            {
                await inventoryLotRepository.InsertAsync(lot);
            }

            // 4. Stock transfer of part of the received stock from main -> second warehouse.
            var secondBalances = new List<InventoryBalance>();
            foreach (var product in products)
            {
                secondBalances.Add(await inventoryBalanceManager.GetOrCreateAsync(secondWarehouse.Id, product.Id));
            }

            var stockTransfer = new StockTransfer(
                Guid.NewGuid(),
                mainWarehouse.Id,
                secondWarehouse.Id,
                products
                    .Select(p => new StockTransferLineData(
                        Guid.NewGuid(), p.Id, new Quantity(TransferredQuantity)))
                    .ToList());
            await stockTransferManager.DispatchAsync(stockTransfer, mainBalances, lots);
            var transferLots = await stockTransferManager.ReceiveAsync(stockTransfer, secondBalances);

            await stockTransferRepository.InsertAsync(stockTransfer);
            foreach (var transferLot in transferLots)
            {
                await inventoryLotRepository.InsertAsync(transferLot);
            }

            // 5. Sale drawn from the remaining main-warehouse stock, confirmed.
            var saleLines = products
                .Select(p => new SaleLineRequest(p.Id, SoldQuantity, UnitPrice))
                .ToList();
            var candidateLots = lots.Where(l => l.RemainingQuantity.Value > 0).ToList();

            var sale = await saleManager.CreateAsync(saleLines, mainBalances, candidateLots);
            await saleManager.ConfirmAsync(sale, mainBalances, candidateLots);

            await saleRepository.InsertAsync(sale, autoSave: true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
