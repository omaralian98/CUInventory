using CUInventory.Catalog.Aggregates;
using CUInventory.Catalog.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CUInventory.EntityFrameworkCore.Configurations.Catalog;

public class ProductConfigurations : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToModuleTable(x => x.Products);
        builder.ConfigureByConvention();


        builder
            .HasOne<Category>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId);

        builder.OwnsOne(x => x.SKU, skuBuilder =>
        {
            skuBuilder
                .Property(x => x.Value)
                .HasMaxLength(256);

            skuBuilder
                .Property(s => s.Value)
                .HasColumnName(nameof(Sku));

            skuBuilder
                .HasIndex(x => x.Value)
                .IsUnique()
                .HasFilter($"{nameof(Sku)} IS NOT NULL");
        });
    }
}
