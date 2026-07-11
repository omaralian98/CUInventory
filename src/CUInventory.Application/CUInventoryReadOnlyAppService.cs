using CUInventory.Localization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace CUInventory;

public abstract class CUInventoryReadOnlyAppService<TEntity, TGetOutputDto, TGetListOutputDto, TKey, TGetListInput>
    : ReadOnlyAppService<TEntity, TGetOutputDto, TGetListOutputDto, TKey, TGetListInput>
    where TEntity : class, IEntity<TKey>
    where TGetOutputDto : IEntityDto<TKey>
    where TGetListOutputDto : IEntityDto<TKey>
{
    protected CUInventoryReadOnlyAppService(IReadOnlyRepository<TEntity, TKey> repository)
        : base(repository)
    {
        LocalizationResource = typeof(CUInventoryResource);
    }
}
