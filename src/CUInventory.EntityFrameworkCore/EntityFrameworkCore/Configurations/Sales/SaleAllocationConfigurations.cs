using CUInventory.Sales.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CUInventory.EntityFrameworkCore.Configurations.Sales;

public class SaleAllocationConfigurations : IEntityTypeConfiguration<SaleAllocation>
{
    public void Configure(EntityTypeBuilder<SaleAllocation> builder)
    {
        builder.ToModuleTable("SaleAllocations");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Ignore(x => x.IsReserved);

        builder.OwnsOne(x => x.Quantity, quantity =>
        {
            quantity
                .Property(x => x.Value)
                .HasColumnName(nameof(SaleAllocation.Quantity))
                .HasColumnType("decimal(18,2)");
        });
        builder.Navigation(x => x.Quantity).IsRequired();

        // UnitCost is null until the reservation is confirmed against a specific lot.
        builder.OwnsOne(x => x.UnitCost, cost =>
        {
            cost
                .Property(x => x.Amount)
                .HasColumnName(nameof(SaleAllocation.UnitCost))
                .HasColumnType("decimal(18,2)");
        });

        // Problem 1: trace which lot/supplier a confirmed sale came from.
        builder.HasIndex(x => x.InventoryLotId);
        builder.HasIndex(x => x.SupplierId);
    }
}
