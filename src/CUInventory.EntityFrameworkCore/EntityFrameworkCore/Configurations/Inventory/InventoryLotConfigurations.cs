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

        builder.HasIndex(x => new { x.ProductId, x.WarehouseId, x.ReceivedAt, x.Id });

        builder.HasIndex(x => x.SupplierId);

        builder.HasIndex(x => x.ShipmentLineId);
        builder.HasIndex(x => x.ReceivedAt);
    }
}
