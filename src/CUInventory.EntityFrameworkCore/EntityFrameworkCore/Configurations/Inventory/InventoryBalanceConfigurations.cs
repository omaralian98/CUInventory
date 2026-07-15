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

        builder.ComplexProperty(x => x.QuantityOnHand, quantity =>
        {
            quantity
                .Property(x => x.Value)
                .HasColumnName(nameof(InventoryBalance.QuantityOnHand))
                .HasColumnType("decimal(18,2)");
        });

        builder.ComplexProperty(x => x.QuantityReserved, quantity =>
        {
            quantity
                .Property(x => x.Value)
                .HasColumnName(nameof(InventoryBalance.QuantityReserved))
                .HasColumnType("decimal(18,2)");
        });

        builder
            .Property(x => x.LowStockThreshold)
            .HasColumnType("decimal(18,2)");

        // One balance row per (warehouse, product): primary lookup AND the guard that
        // prevents concurrent get-or-create from producing duplicate balance rows.
        builder
            .HasIndex(x => new { x.WarehouseId, x.ProductId })
            .IsUnique();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_AppInventoryBalances_QuantityOnHand_NonNegative", "QuantityOnHand >= 0");
            t.HasCheckConstraint("CK_AppInventoryBalances_QuantityReserved_NonNegative", "QuantityReserved >= 0");
            t.HasCheckConstraint("CK_AppInventoryBalances_QuantityReserved_WithinOnHand", "QuantityReserved <= QuantityOnHand");
        });
    }
}
