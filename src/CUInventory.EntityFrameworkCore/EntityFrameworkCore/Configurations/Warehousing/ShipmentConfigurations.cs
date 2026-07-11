using CUInventory.Warehousing.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CUInventory.EntityFrameworkCore.Configurations.Warehousing;

public class ShipmentConfigurations : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToModuleTable(x => x.Shipments);
        builder.ConfigureByConvention();

        builder
            .HasIndex(x => x.PurchaseOrderId);

        builder
            .HasIndex(x => x.Status);

        builder
            .HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(x => x.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
