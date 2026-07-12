using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Dtos;
using CUInventory.ValueObjects;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace CUInventory.Mappers.Inventory;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class StockTransferMapper : MapperBase<StockTransfer, StockTransferDto>
{
    public override partial StockTransferDto Map(StockTransfer source);

    public override partial void Map(StockTransfer source, StockTransferDto destination);

    private static decimal MapQuantity(Quantity quantity) => quantity.Value;

    private static decimal MapMoney(Money money) => money.Amount;
}
