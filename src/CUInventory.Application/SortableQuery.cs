using System.Linq;
using System.Linq.Dynamic.Core;
using CUInventory.Abstractions;
using Volo.Abp.Application.Dtos;

namespace CUInventory;

internal static class SortableQuery
{
    public static IQueryable<TEntity>? TryApplyDefaultOrderIndex<TEntity>(IQueryable<TEntity> query, object? input)
    {
        if (!typeof(ISortable).IsAssignableFrom(typeof(TEntity)))
        {
            return null;
        }

        if (input is ISortedResultRequest { Sorting: var sorting } && !string.IsNullOrWhiteSpace(sorting))
        {
            return null;
        }
        var ordering = typeof(TEntity).GetProperty("Id") != null
            ? $"{nameof(ISortable.OrderIndex)}, Id"
            : nameof(ISortable.OrderIndex);

        return query.OrderBy(ordering);
    }
}
