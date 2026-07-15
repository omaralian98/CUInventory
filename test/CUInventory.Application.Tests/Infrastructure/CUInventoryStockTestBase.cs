using System;
using System.Threading.Tasks;
using CUInventory.Catalog;
using CUInventory.Catalog.Dtos;
using CUInventory.Inventory;
using CUInventory.Inventory.Dtos;
using CUInventory.Procurement;
using CUInventory.Procurement.Dtos;
using CUInventory.Sales;
using CUInventory.Shared.Dtos;
using CUInventory.Warehousing;
using CUInventory.Warehousing.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;

namespace CUInventory;

/// <summary>
/// Base class for application tests that need real stock. Stock can only enter the system through the
/// procurement -> warehousing -> inventory chain (the balance mutators are internal), so seeding drives
/// the actual PurchaseOrder -> Shipment -> Receive flow. This doubles as end-to-end coverage of that chain.
/// </summary>
public abstract class CUInventoryStockTestBase<TStartupModule> : CUInventoryApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    protected readonly IWarehouseAppService WarehouseAppService;
    protected readonly IProductAppService ProductAppService;
    protected readonly ISupplierAppService SupplierAppService;
    protected readonly IPurchaseOrderAppService PurchaseOrderAppService;
    protected readonly IShipmentAppService ShipmentAppService;
    protected readonly IStockTransferAppService StockTransferAppService;
    protected readonly IInventoryBalanceAppService InventoryBalanceAppService;
    protected readonly IInventoryLotAppService InventoryLotAppService;
    protected readonly ISaleAppService SaleAppService;

    protected CUInventoryStockTestBase()
    {
        WarehouseAppService = GetRequiredService<IWarehouseAppService>();
        ProductAppService = GetRequiredService<IProductAppService>();
        SupplierAppService = GetRequiredService<ISupplierAppService>();
        PurchaseOrderAppService = GetRequiredService<IPurchaseOrderAppService>();
        ShipmentAppService = GetRequiredService<IShipmentAppService>();
        StockTransferAppService = GetRequiredService<IStockTransferAppService>();
        InventoryBalanceAppService = GetRequiredService<IInventoryBalanceAppService>();
        InventoryLotAppService = GetRequiredService<IInventoryLotAppService>();
        SaleAppService = GetRequiredService<ISaleAppService>();
    }

    protected static ConcurrencyStampDto Stamp(IHasConcurrencyStamp dto) =>
        new() { ConcurrencyStamp = dto.ConcurrencyStamp };

    protected static AddressDto SampleAddress() =>
        new() { Governorate = "Damascus", City = "Damascus", Street = "Main St" };

    protected async Task<Guid> SeedWarehouseAsync(string? name = null, string? code = null)
    {
        var token = Guid.NewGuid().ToString("N");
        var warehouse = await WarehouseAppService.CreateAsync(new CreateWarehouseDto
        {
            Name = name ?? $"WH-{token}",
            Code = code ?? $"WH-{token[..12]}",
            Address = SampleAddress()
        });
        return warehouse.Id;
    }

    protected async Task<Guid> SeedProductAsync(string? name = null, string? sku = null, Guid? categoryId = null)
    {
        var token = Guid.NewGuid().ToString("N");
        var product = await ProductAppService.CreateAsync(new CreateProductDto
        {
            Name = name ?? $"Product-{token}",
            Sku = sku,
            CategoryId = categoryId
        });
        return product.Id;
    }

    protected async Task<Guid> SeedSupplierAsync()
    {
        var token = Guid.NewGuid().ToString("N")[..12];
        var supplier = await SupplierAppService.CreateAsync(new CreateSupplierDto
        {
            Name = $"Supplier-{token}",
            Contact = new ContactInfoDto
            {
                Email = $"s{token}@example.com",
                PhoneNumber = "+963112345678",
                Address = SampleAddress()
            }
        });
        return supplier.Id;
    }

    /// <summary>
    /// Places <paramref name="quantity"/> units of <paramref name="productId"/> on hand at
    /// <paramref name="warehouseId"/> by running a full purchase-order + shipment receipt.
    /// Returns the id of the inventory lot created at the destination.
    /// </summary>
    protected async Task<Guid> SeedStockAsync(
        Guid warehouseId,
        Guid productId,
        decimal quantity,
        decimal unitCost = 5m,
        Guid? supplierId = null)
    {
        var supplier = supplierId ?? Guid.NewGuid();

        var order = await PurchaseOrderAppService.CreateAsync(new CreatePurchaseOrderDto
        {
            SupplierId = supplier,
            DestinationWarehouseId = warehouseId,
            Lines =
            {
                new CreatePurchaseOrderLineDto
                {
                    ProductId = productId, OrderedQuantity = quantity, UnitCost = unitCost
                }
            }
        });
        await PurchaseOrderAppService.ConfirmAsync(order.Id, await StampOfPurchaseOrderAsync(order.Id));

        var shipment = await ShipmentAppService.CreateAsync(new CreateShipmentDto
        {
            PurchaseOrderId = order.Id,
            SupplierId = supplier,
            DestinationWarehouseId = warehouseId,
            Lines =
            {
                new CreateShipmentLineDto { ProductId = productId, Quantity = quantity, UnitCost = unitCost }
            }
        });
        await ShipmentAppService.DispatchAsync(shipment.Id, await StampOfShipmentAsync(shipment.Id));
        await ShipmentAppService.ReceiveAsync(shipment.Id, await StampOfShipmentAsync(shipment.Id));

        var lots = await InventoryLotAppService.GetListAsync(new GetInventoryLotListDto
        {
            WarehouseId = warehouseId,
            ProductId = productId
        });

        return lots.Items[^1].Id;
    }

    protected async Task<Guid> FindBalanceIdAsync(Guid warehouseId, Guid productId)
    {
        var balances = await InventoryBalanceAppService.GetListAsync(new GetInventoryBalanceListDto
        {
            WarehouseId = warehouseId,
            ProductId = productId
        });
        return balances.Items[0].Id;
    }

    // Lifecycle methods require the current persisted stamp. CreateAsync does not auto-save, so the stamp on
    // its returned DTO is not yet the persisted one; always re-read before a lifecycle call.
    protected async Task<ConcurrencyStampDto> StampOfPurchaseOrderAsync(Guid id) =>
        Stamp(await PurchaseOrderAppService.GetAsync(id));

    protected async Task<ConcurrencyStampDto> StampOfShipmentAsync(Guid id) =>
        Stamp(await ShipmentAppService.GetAsync(id));

    protected async Task<ConcurrencyStampDto> StampOfSaleAsync(Guid id) =>
        Stamp(await SaleAppService.GetAsync(id));

    protected async Task<ConcurrencyStampDto> StampOfTransferAsync(Guid id) =>
        Stamp(await StockTransferAppService.GetAsync(id));

    protected async Task<ConcurrencyStampDto> StampOfBalanceAsync(Guid id) =>
        Stamp(await InventoryBalanceAppService.GetAsync(id));
}
