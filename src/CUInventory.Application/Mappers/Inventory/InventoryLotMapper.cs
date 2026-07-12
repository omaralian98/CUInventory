using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Dtos;
using CUInventory.ValueObjects;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace CUInventory.Mappers.Inventory;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class InventoryLotMapper : MapperBase<InventoryLot, InventoryLotDto>
{
    public override partial InventoryLotDto Map(InventoryLot source);

    public override partial void Map(InventoryLot source, InventoryLotDto destination);

    private static decimal MapQuantity(Quantity quantity) => quantity.Value;

    private static decimal MapMoney(Money money) => money.Amount;
}
