using CUInventory.Procurement.Aggregates;
using CUInventory.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CUInventory.EntityFrameworkCore.Configurations.Procurement;

public class SupplierConfigurations : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToModuleTable(x => x.Suppliers);
        builder.ConfigureByConvention();

        builder
            .Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.OwnsOne(x => x.Contact, contact =>
        {
            contact.OwnsOne(c => c.Email, email =>
            {
                email
                    .Property(e => e.Value)
                    .HasColumnName(nameof(Supplier.Contact.Email))
                    .HasMaxLength(256)
                    .IsRequired();

                email
                    .HasIndex(e => e.Value)
                    .IsUnique();
            });
            contact.Navigation(c => c.Email).IsRequired();

            contact.OwnsOne(c => c.PhoneNumber, phone =>
            {
                phone
                    .Property(p => p.Value)
                    .HasColumnName(nameof(Supplier.Contact.PhoneNumber))
                    .HasMaxLength(32)
                    .IsRequired();

                phone
                    .HasIndex(p => p.Value)
                    .IsUnique();
            });
            contact.Navigation(c => c.PhoneNumber).IsRequired();

            contact.OwnsOne(c => c.Address, address =>
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
            contact.Navigation(c => c.Address).IsRequired();
        });
        builder.Navigation(x => x.Contact).IsRequired();
    }
}
