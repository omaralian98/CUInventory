using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Procurement.Aggregates;

namespace CUInventory.Procurement.Interfaces;

public interface ISupplierManager : IDomainService
{
    Task<Supplier> CreateAsync(/* TODO: parameters once Supplier has properties */);
}
