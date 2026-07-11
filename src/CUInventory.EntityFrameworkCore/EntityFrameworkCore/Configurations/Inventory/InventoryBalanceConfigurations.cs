using CUInventory.Inventory.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CUInventory.EntityFrameworkCore.Configurations.Inventory;

public class InventoryBalanceConfigurations : IEntityTypeConfiguration<InventoryBalance>
{
    public void Configure(EntityTypeBuilder<InventoryBalance> builder)
    {
        builder.ToModuleTable(x => x.InventoryBalances);
        builder.ConfigureByConvention();

        // QuantityAvailable is derived (OnHand - Reserved); never persisted.
        builder.Ignore(x => x.QuantityAvailable);

        builder
            .Property(x => x.QuantityOnHand)
            .HasColumnType("decimal(18,2)");

        builder
            .Property(x => x.QuantityReserved)
            .HasColumnType("decimal(18,2)");

        builder
            .Property(x => x.LowStockThreshold)
            .HasColumnType("decimal(18,2)");

        // One balance row per (warehouse, product): primary lookup AND the guard that
        // prevents concurrent get-or-create from producing duplicate balance rows.
        builder
            .HasIndex(x => new { x.WarehouseId, x.ProductId })
            .IsUnique();
    }
}
