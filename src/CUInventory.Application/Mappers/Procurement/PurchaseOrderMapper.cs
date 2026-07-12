using CUInventory.Procurement.Aggregates;
using CUInventory.Procurement.Dtos;
using CUInventory.ValueObjects;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace CUInventory.Mappers.Procurement;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class PurchaseOrderMapper : MapperBase<PurchaseOrder, PurchaseOrderDto>
{
    public override partial PurchaseOrderDto Map(PurchaseOrder source);

    public override partial void Map(PurchaseOrder source, PurchaseOrderDto destination);

    private static decimal MapQuantity(Quantity quantity) => quantity.Value;

    private static decimal MapMoney(Money money) => money.Amount;
}
