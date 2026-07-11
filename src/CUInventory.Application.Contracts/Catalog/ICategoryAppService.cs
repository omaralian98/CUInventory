using System;
using CUInventory.Catalog.Dtos;
using Volo.Abp.Application.Services;

namespace CUInventory.Catalog;

public interface ICategoryAppService :
    ICrudAppService<CategoryDto, Guid, GetCategoryListDto, CreateCategoryDto, UpdateCategoryDto>
{
}
