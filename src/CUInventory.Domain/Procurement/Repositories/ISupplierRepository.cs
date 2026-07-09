using System;
using Volo.Abp.Domain.Repositories;
using CUInventory.Procurement.Aggregates;

namespace CUInventory.Procurement.Repositories;

public interface ISupplierRepository : IRepository<Supplier, Guid>
{
}
