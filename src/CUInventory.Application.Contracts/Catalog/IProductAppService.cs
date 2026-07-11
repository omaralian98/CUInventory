using System;
using CUInventory.Catalog.Dtos;
using Volo.Abp.Application.Services;

namespace CUInventory.Catalog;

public interface IProductAppService :
    ICrudAppService<ProductDto, Guid, GetProductListDto, CreateProductDto, UpdateProductDto>
{
}
