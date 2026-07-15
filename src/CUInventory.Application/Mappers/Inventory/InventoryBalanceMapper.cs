using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Dtos;
using CUInventory.ValueObjects;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace CUInventory.Mappers.Inventory;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class InventoryBalanceMapper : MapperBase<InventoryBalance, InventoryBalanceDto>
{
    public override partial InventoryBalanceDto Map(InventoryBalance source);

    public override partial void Map(InventoryBalance source, InventoryBalanceDto destination);

    private static decimal MapQuantity(Quantity quantity) => quantity.Value;
}
