using CUInventory.Inventory.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CUInventory.EntityFrameworkCore.Configurations.Inventory;

public class InventoryLotConfigurations : IEntityTypeConfiguration<InventoryLot>
{
    public void Configure(EntityTypeBuilder<InventoryLot> builder)
    {
        builder.ToModuleTable(x => x.InventoryLots);
        builder.ConfigureByConvention();

        builder.ComplexProperty(x => x.OriginalQuantity, quantity =>
        {
            quantity
                .Property(x => x.Value)
                .HasColumnName(nameof(InventoryLot.OriginalQuantity))
                .HasColumnType("decimal(18,2)");
        });

        builder.ComplexProperty(x => x.RemainingQuantity, quantity =>
        {
            quantity
                .Property(x => x.Value)
                .HasColumnName(nameof(InventoryLot.RemainingQuantity))
                .HasColumnType("decimal(18,2)");
        });

        builder.ComplexProperty(x => x.UnitCost, cost =>
        {
            cost
                .Property(x => x.Amount)
                .HasColumnName(nameof(InventoryLot.UnitCost))
                .HasColumnType("decimal(18,2)");
        });

        // FIFO allocation ordering + per-product/warehouse availability scans.
        builder.HasIndex(x => new { x.ProductId, x.WarehouseId, x.ReceivedAt, x.Id });

        // Problem 1: "how much did we sell that came from supplier Y".
        builder.HasIndex(x => x.SupplierId);

        // Traceability back to the receiving shipment line.
        builder.HasIndex(x => x.ShipmentLineId);
    }
}
