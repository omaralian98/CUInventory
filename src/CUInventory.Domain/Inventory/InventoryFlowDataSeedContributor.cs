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

public class InventoryFlowDataSeedContributor(
    IClock clock,
    IUnitOfWorkManager unitOfWorkManager,
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
    ISaleRepository saleRepository,
    Catalog.CatalogDataSeedContributor catalogSeeder,
    ProcurementDataSeedContributor procurementSeeder,
    WarehousingDataSeedContributor warehousingSeeder)
    : IDataSeedContributor, ITransientDependency
{
    private const int SagaCount = 12;
    private const int ProductsPerSaga = 3;
    private const decimal PurchasedQuantity = 100m;
    private const decimal TransferredQuantity = 20m;
    private const decimal SoldQuantity = 10m;

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        if (await saleRepository.GetCountAsync() > 0)
        {
            return;
        }

        await warehousingSeeder.SeedAsync(context);
        await catalogSeeder.SeedAsync(context);
        await procurementSeeder.SeedAsync(context);

        var warehouses = await warehouseRepository.GetListAsync();
        var products = (await productRepository.GetListAsync())
            .Where(p => !p.IsService)
            .ToList();
        var suppliers = await supplierRepository.GetListAsync();

        if (warehouses.Count < 2 || products.Count == 0 || suppliers.Count == 0)
        {
            Console.WriteLine("InventoryFlowDataSeedContributor: prerequisites missing after seeding catalog/procurement/warehousing. Skipping.");
            return;
        }

        for (var i = 0; i < SagaCount; i++)
        {
            // Each saga is its own unit of work: a failed iteration rolls back on its own without
            // poisoning the change tracker for the others.
            using var uow = unitOfWorkManager.Begin(requiresNew: true);
            try
            {
                await SeedSagaAsync(i, warehouses, products, suppliers);
                await uow.CompleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private async Task SeedSagaAsync(
        int index,
        IReadOnlyList<Warehouse> warehouses,
        IReadOnlyList<Product> products,
        IReadOnlyList<Supplier> suppliers)
    {
        var supplier = suppliers[index % suppliers.Count];
        var mainWarehouse = warehouses[index % warehouses.Count];
        var secondWarehouse = warehouses[(index + 1) % warehouses.Count];

        var chosenProducts = Enumerable.Range(0, Math.Min(ProductsPerSaga, products.Count))
            .Select(j => products[(index + j) % products.Count])
            .DistinctBy(p => p.Id)
            .ToList();

        var now = clock.Now;
        var unitCost = 10m + index % 5;
        var unitPrice = 15m + (index % 5) * 2;

        // 1. Purchase order for the selected products, confirmed and ready to receive.
        var purchaseOrder = new PurchaseOrder(
            Guid.NewGuid(),
            supplier.Id,
            mainWarehouse.Id,
            chosenProducts
                .Select(p => new PurchaseOrderLineData(
                    Guid.NewGuid(), p.Id, new Quantity(PurchasedQuantity), new Money(unitCost)))
                .ToList());
        purchaseOrder.Confirm(now);
        await purchaseOrderRepository.InsertAsync(purchaseOrder);

        // 2. Inventory balances at the destination warehouse.
        var mainBalances = new List<InventoryBalance>();
        foreach (var product in chosenProducts)
        {
            mainBalances.Add(await inventoryBalanceManager.GetOrCreateAsync(mainWarehouse.Id, product.Id));
        }

        // 3. Shipment against the purchase order -> creates lots, raises balances, registers receipt.
        var shipment = new Shipment(
            Guid.NewGuid(),
            purchaseOrder.Id,
            supplier.Id,
            mainWarehouse.Id,
            chosenProducts
                .Select(p => new ShipmentLineData(
                    Guid.NewGuid(), p.Id, new Quantity(PurchasedQuantity), new Money(unitCost)))
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
        foreach (var product in chosenProducts)
        {
            secondBalances.Add(await inventoryBalanceManager.GetOrCreateAsync(secondWarehouse.Id, product.Id));
        }

        var stockTransfer = new StockTransfer(
            Guid.NewGuid(),
            mainWarehouse.Id,
            secondWarehouse.Id,
            chosenProducts
                .Select(p => new StockTransferLineData(
                    Guid.NewGuid(), p.Id, new Quantity(TransferredQuantity)))
                .ToList());
        await stockTransferManager.DispatchAsync(stockTransfer, mainBalances, lots);
        var transferLots = await stockTransferManager.ReceiveAsync(stockTransfer, secondBalances, lots);

        await stockTransferRepository.InsertAsync(stockTransfer);
        foreach (var transferLot in transferLots)
        {
            await inventoryLotRepository.InsertAsync(transferLot);
        }

        // 5. Sale drawn from the remaining main-warehouse stock, confirmed.
        var saleLines = chosenProducts
            .Select(p => new SaleLineRequest(p.Id, SoldQuantity, unitPrice))
            .ToList();
        var candidateLots = lots.Where(l => l.AvailableQuantity > 0).ToList();

        var sale = await saleManager.CreateAsync(saleLines, mainBalances, candidateLots);
        await saleManager.ConfirmAsync(sale, mainBalances, candidateLots);

        await saleRepository.InsertAsync(sale, autoSave: true);
    }
}
