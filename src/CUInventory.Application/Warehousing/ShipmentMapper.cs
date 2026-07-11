using CUInventory.ValueObjects;
using CUInventory.Warehousing.Aggregates;
using CUInventory.Warehousing.Dtos;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace CUInventory.Warehousing;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ShipmentMapper : MapperBase<Shipment, ShipmentDto>
{
    public override partial ShipmentDto Map(Shipment source);

    public override partial void Map(Shipment source, ShipmentDto destination);

    private static decimal MapQuantity(Quantity quantity) => quantity.Value;

    private static decimal MapMoney(Money money) => money.Amount;
}
