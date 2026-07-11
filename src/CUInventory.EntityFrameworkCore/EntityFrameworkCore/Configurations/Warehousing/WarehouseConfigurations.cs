using CUInventory.ValueObjects;
using CUInventory.Warehousing.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CUInventory.EntityFrameworkCore.Configurations.Warehousing;

public class WarehouseConfigurations : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToModuleTable(x => x.Warehouses);
        builder.ConfigureByConvention();

        builder
            .Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder
            .Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(64);

        builder
            .HasIndex(x => x.Code)
            .IsUnique();

        builder.OwnsOne(x => x.Address, address =>
        {
            address
                .Property(a => a.Governorate)
                .HasColumnName(nameof(Address.Governorate))
                .HasMaxLength(128)
                .IsRequired();

            address
                .Property(a => a.City)
                .HasColumnName(nameof(Address.City))
                .HasMaxLength(128)
                .IsRequired();

            address
                .Property(a => a.Street)
                .HasColumnName(nameof(Address.Street))
                .HasMaxLength(256)
                .IsRequired();
        });
        builder.Navigation(x => x.Address).IsRequired();
    }
}
