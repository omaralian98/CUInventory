using CUInventory.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CUInventory.EntityFrameworkCore.Configurations.Procurement;

public class PurchaseOrderLineConfigurations : IEntityTypeConfiguration<PurchaseOrderLine>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderLine> builder)
    {
        builder.ToModuleTable("PurchaseOrderLines");

        builder.ConfigureByConvention();
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Ignore(x => x.OutstandingQuantity);
        builder.Ignore(x => x.IsFullyReceived);

        builder.ComplexProperty(x => x.OrderedQuantity, quantity =>
        {
            quantity
                .Property(x => x.Value)
                .HasColumnName(nameof(PurchaseOrderLine.OrderedQuantity))
                .HasColumnType("decimal(18,2)");
        });

        builder.ComplexProperty(x => x.ReceivedQuantity, quantity =>
        {
            quantity
                .Property(x => x.Value)
                .HasColumnName(nameof(PurchaseOrderLine.ReceivedQuantity))
                .HasColumnType("decimal(18,2)");
        });

        builder.ComplexProperty(x => x.UnitCost, cost =>
        {
            cost
                .Property(x => x.Amount)
                .HasColumnName(nameof(PurchaseOrderLine.UnitCost))
                .HasColumnType("decimal(18,2)");
        });

        builder.HasIndex(x => x.ProductId);
    }
}
