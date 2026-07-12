using CUInventory.Catalog.Aggregates;
using CUInventory.Catalog.Dtos;
using CUInventory.Catalog.ValueObjects;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace CUInventory.Mappers.Catalog;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ProductMapper : MapperBase<Product, ProductDto>
{
    [MapProperty(nameof(Product.SKU), nameof(ProductDto.Sku))]
    public override partial ProductDto Map(Product source);

    [MapProperty(nameof(Product.SKU), nameof(ProductDto.Sku))]
    public override partial void Map(Product source, ProductDto destination);

    private static string? MapSku(Sku? sku) => sku?.Value;
}
