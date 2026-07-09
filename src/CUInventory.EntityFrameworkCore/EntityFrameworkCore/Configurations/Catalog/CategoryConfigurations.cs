using CUInventory.Catalog.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CUInventory.EntityFrameworkCore.Configurations.Catalog;

public class CategoryConfigurations : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToModuleTable(x => x.Categories);
        builder.ConfigureByConvention();

        builder
            .HasIndex(x => x.Name)
            .IsUnique();
    }
}