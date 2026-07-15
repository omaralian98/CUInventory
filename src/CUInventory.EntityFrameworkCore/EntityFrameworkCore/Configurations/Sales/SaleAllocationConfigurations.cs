using CUInventory.Sales.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CUInventory.EntityFrameworkCore.Configurations.Sales;

public class SaleAllocationConfigurations : IEntityTypeConfiguration<SaleAllocation>
{
    public void Configure(EntityTypeBuilder<SaleAllocation> builder)
    {
        builder.ToModuleTable("SaleAllocations");

        builder.ConfigureByConvention();
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.ComplexProperty(x => x.Quantity, quantity =>
        {
            quantity
                .Property(x => x.Value)
                .HasColumnName(nameof(SaleAllocation.Quantity))
                .HasColumnType("decimal(18,2)");
        });

        builder.ComplexProperty(x => x.UnitCost, cost =>
        {
            cost.IsRequired(false);
            cost
                .Property(x => x.Amount)
                .HasColumnName(nameof(SaleAllocation.UnitCost))
                .HasColumnType("decimal(18,2)");
        });

        builder.HasIndex(x => x.InventoryLotId);
        builder.HasIndex(x => x.SupplierId);
    }
}
