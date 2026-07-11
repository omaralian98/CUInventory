using CUInventory.Warehousing.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CUInventory.EntityFrameworkCore.Configurations.Warehousing;

public class ShipmentLineConfigurations : IEntityTypeConfiguration<ShipmentLine>
{
    public void Configure(EntityTypeBuilder<ShipmentLine> builder)
    {
        builder.ToModuleTable("ShipmentLines");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.OwnsOne(x => x.Quantity, quantity =>
        {
            quantity
                .Property(x => x.Value)
                .HasColumnName(nameof(ShipmentLine.Quantity))
                .HasColumnType("decimal(18,2)");
        });
        builder.Navigation(x => x.Quantity).IsRequired();

        builder.OwnsOne(x => x.UnitCost, cost =>
        {
            cost
                .Property(x => x.Amount)
                .HasColumnName(nameof(ShipmentLine.UnitCost))
                .HasColumnType("decimal(18,2)");
        });
        builder.Navigation(x => x.UnitCost).IsRequired();

        builder.HasIndex(x => x.ProductId);
    }
}
