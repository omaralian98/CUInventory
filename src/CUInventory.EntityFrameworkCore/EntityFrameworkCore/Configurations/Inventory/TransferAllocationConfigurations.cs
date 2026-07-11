using CUInventory.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CUInventory.EntityFrameworkCore.Configurations.Inventory;

public class TransferAllocationConfigurations : IEntityTypeConfiguration<TransferAllocation>
{
    public void Configure(EntityTypeBuilder<TransferAllocation> builder)
    {
        builder.ToModuleTable("TransferAllocations");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.OwnsOne(x => x.Quantity, quantity =>
        {
            quantity
                .Property(x => x.Value)
                .HasColumnName(nameof(TransferAllocation.Quantity))
                .HasColumnType("decimal(18,2)");
        });
        builder.Navigation(x => x.Quantity).IsRequired();

        builder.OwnsOne(x => x.UnitCost, cost =>
        {
            cost
                .Property(x => x.Amount)
                .HasColumnName(nameof(TransferAllocation.UnitCost))
                .HasColumnType("decimal(18,2)");
        });
        builder.Navigation(x => x.UnitCost).IsRequired();

        builder.HasIndex(x => x.SourceLotId);
    }
}
