using CUInventory.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CUInventory.EntityFrameworkCore.Configurations.Procurement;

public class PurchaseOrderLineConfigurations : IEntityTypeConfiguration<PurchaseOrderLine>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderLine> builder)
    {
        builder.ToModuleTable("PurchaseOrderLines");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Ignore(x => x.OutstandingQuantity);
        builder.Ignore(x => x.IsFullyReceived);

        builder.OwnsOne(x => x.OrderedQuantity, quantity =>
        {
            quantity
                .Property(x => x.Value)
                .HasColumnName(nameof(PurchaseOrderLine.OrderedQuantity))
                .HasColumnType("decimal(18,2)");
        });
        builder.Navigation(x => x.OrderedQuantity).IsRequired();

        builder.OwnsOne(x => x.ReceivedQuantity, quantity =>
        {
            quantity
                .Property(x => x.Value)
                .HasColumnName(nameof(PurchaseOrderLine.ReceivedQuantity))
                .HasColumnType("decimal(18,2)");
        });
        builder.Navigation(x => x.ReceivedQuantity).IsRequired();

        builder.OwnsOne(x => x.UnitCost, cost =>
        {
            cost
                .Property(x => x.Amount)
                .HasColumnName(nameof(PurchaseOrderLine.UnitCost))
                .HasColumnType("decimal(18,2)");
        });
        builder.Navigation(x => x.UnitCost).IsRequired();

        builder.HasIndex(x => x.ProductId);
    }
}
