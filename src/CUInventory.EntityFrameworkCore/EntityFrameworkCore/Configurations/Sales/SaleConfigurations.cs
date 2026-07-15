using CUInventory.Sales.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CUInventory.EntityFrameworkCore.Configurations.Sales;

public class SaleConfigurations : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToModuleTable(x => x.Sales);
        builder.ConfigureByConvention();

        builder.ComplexProperty(x => x.TotalAmount, total =>
        {
            total
                .Property(x => x.Amount)
                .HasColumnName(nameof(Sale.TotalAmount))
                .HasColumnType("decimal(18,2)");
        });

        builder
            .HasIndex(x => new { x.Status, x.ConfirmedAt });

        builder
            .HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(x => x.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
