using System;
using Volo.Abp.Domain.Repositories;
using CUInventory.Sales.Aggregates;

namespace CUInventory.Sales.Repositories;

public interface ISaleRepository : IRepository<Sale, Guid>
{
}
