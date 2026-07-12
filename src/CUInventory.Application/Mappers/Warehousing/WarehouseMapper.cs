using CUInventory.Warehousing.Aggregates;
using CUInventory.Warehousing.Dtos;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace CUInventory.Mappers.Warehousing;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class WarehouseMapper : MapperBase<Warehouse, WarehouseDto>
{
    public override partial WarehouseDto Map(Warehouse source);

    public override partial void Map(Warehouse source, WarehouseDto destination);
}
