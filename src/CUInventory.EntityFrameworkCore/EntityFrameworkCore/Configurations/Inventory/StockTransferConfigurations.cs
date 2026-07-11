using CUInventory.Inventory.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CUInventory.EntityFrameworkCore.Configurations.Inventory;

public class StockTransferConfigurations : IEntityTypeConfiguration<StockTransfer>
{
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.ToModuleTable(x => x.StockTransfers);
        builder.ConfigureByConvention();

        // Recovery/monitoring: find transfers stuck in-transit (Dispatched) after a crash.
        builder
            .HasIndex(x => x.Status);

        builder
            .HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.StockTransferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(x => x.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder
            .HasMany(x => x.Allocations)
            .WithOne()
            .HasForeignKey(x => x.StockTransferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(x => x.Allocations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
