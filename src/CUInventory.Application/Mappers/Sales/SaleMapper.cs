using CUInventory.Sales.Aggregates;
using CUInventory.Sales.Dtos;
using CUInventory.ValueObjects;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace CUInventory.Mappers.Sales;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SaleMapper : MapperBase<Sale, SaleDto>
{
    public override partial SaleDto Map(Sale source);

    public override partial void Map(Sale source, SaleDto destination);

    private static decimal MapQuantity(Quantity quantity) => quantity.Value;

    private static decimal MapMoney(Money money) => money.Amount;

    private static decimal? MapMoneyNullable(Money? money) => money?.Amount;
}
