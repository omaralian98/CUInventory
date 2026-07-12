using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Abstractions;
using CUInventory.Localization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.ChangeTracking;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace CUInventory;

public abstract class CUInventoryCrudAppService<TEntity, TGetOutputDto, TGetListOutputDto, TKey, TGetListInput,
    TCreateInput, TUpdateInput>
    : CrudAppService<TEntity, TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput>
    where TEntity : class, IEntity<TKey>
    where TGetOutputDto : IEntityDto<TKey>
    where TGetListOutputDto : IEntityDto<TKey>
{
    protected CUInventoryCrudAppService(IRepository<TEntity, TKey> repository)
        : base(repository)
    {
        LocalizationResource = typeof(CUInventoryResource);
    }

    [DisableEntityChangeTracking]
    public override Task<TGetOutputDto> GetAsync(TKey id)
    {
        return base.GetAsync(id);
    }

    [DisableEntityChangeTracking]
    public override async Task<PagedResultDto<TGetListOutputDto>> GetListAsync(TGetListInput input)
    {
        if (input is IHasIncludeInactive { IncludeInactive: true })
        {
            using (DataFilter.Disable<IIsActive>())
            {
                return await base.GetListAsync(input);
            }
        }

        using (DataFilter.Enable<IIsActive>())
        {
            return await base.GetListAsync(input);
        }
    }

    protected override IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, TGetListInput input)
    {
        return SortableQuery.TryApplyDefaultOrderIndex(query, input) ?? base.ApplySorting(query, input);
    }
}

public abstract class CUInventoryCrudAppService<TEntity, TGetOutputDto, TGetListOutputDto, TKey, TGetListInput,
    TCreateInput>(
    IRepository<TEntity, TKey> repository)
    : CUInventoryCrudAppService<TEntity, TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput,
        TCreateInput>(repository)
    where TEntity : class, IEntity<TKey>
    where TGetOutputDto : IEntityDto<TKey>
    where TGetListOutputDto : IEntityDto<TKey>
{
    [RemoteService(false)]
    public override Task<TGetOutputDto> UpdateAsync(TKey id, TCreateInput input)
    {
        throw new NotSupportedException();
    }
}