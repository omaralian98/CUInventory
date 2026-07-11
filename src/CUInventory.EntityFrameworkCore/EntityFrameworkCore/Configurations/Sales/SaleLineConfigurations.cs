using CUInventory.Sales.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CUInventory.EntityFrameworkCore.Configurations.Sales;

public class SaleLineConfigurations : IEntityTypeConfiguration<SaleLine>
{
    public void Configure(EntityTypeBuilder<SaleLine> builder)
    {
        builder.ToModuleTable("SaleLines");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.OwnsOne(x => x.Quantity, quantity =>
        {
            quantity
                .Property(x => x.Value)
                .HasColumnName(nameof(SaleLine.Quantity))
                .HasColumnType("decimal(18,2)");
        });
        builder.Navigation(x => x.Quantity).IsRequired();

        builder.OwnsOne(x => x.UnitPrice, price =>
        {
            price
                .Property(x => x.Amount)
                .HasColumnName(nameof(SaleLine.UnitPrice))
                .HasColumnType("decimal(18,2)");
        });
        builder.Navigation(x => x.UnitPrice).IsRequired();

        builder.HasIndex(x => x.ProductId);

        builder
            .HasMany(x => x.Allocations)
            .WithOne()
            .HasForeignKey(x => x.SaleLineId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(x => x.Allocations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
