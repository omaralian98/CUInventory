using CUInventory.Catalog.Aggregates;
using CUInventory.Catalog.Dtos;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace CUInventory.Mappers.Catalog;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class CategoryMapper : MapperBase<Category, CategoryDto>
{
    public override partial CategoryDto Map(Category source);

    public override partial void Map(Category source, CategoryDto destination);
}
