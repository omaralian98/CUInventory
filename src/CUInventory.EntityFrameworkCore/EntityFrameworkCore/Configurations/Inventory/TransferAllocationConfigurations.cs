using CUInventory.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CUInventory.EntityFrameworkCore.Configurations.Inventory;

public class TransferAllocationConfigurations : IEntityTypeConfiguration<TransferAllocation>
{
    public void Configure(EntityTypeBuilder<TransferAllocation> builder)
    {
        builder.ToModuleTable("TransferAllocations");

        builder.ConfigureByConvention();
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.ComplexProperty(x => x.Quantity, quantity =>
        {
            quantity
                .Property(x => x.Value)
                .HasColumnName(nameof(TransferAllocation.Quantity))
                .HasColumnType("decimal(18,2)");
        });

        builder.ComplexProperty(x => x.UnitCost, cost =>
        {
            cost
                .Property(x => x.Amount)
                .HasColumnName(nameof(TransferAllocation.UnitCost))
                .HasColumnType("decimal(18,2)");
        });

        builder.HasIndex(x => x.SourceLotId);
    }
}
