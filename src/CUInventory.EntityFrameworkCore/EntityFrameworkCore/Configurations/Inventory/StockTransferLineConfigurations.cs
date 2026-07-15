using CUInventory.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CUInventory.EntityFrameworkCore.Configurations.Inventory;

public class StockTransferLineConfigurations : IEntityTypeConfiguration<StockTransferLine>
{
    public void Configure(EntityTypeBuilder<StockTransferLine> builder)
    {
        builder.ToModuleTable("StockTransferLines");

        builder.ConfigureByConvention();
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.ComplexProperty(x => x.Quantity, quantity =>
        {
            quantity
                .Property(x => x.Value)
                .HasColumnName(nameof(StockTransferLine.Quantity))
                .HasColumnType("decimal(18,2)");
        });
    }
}
