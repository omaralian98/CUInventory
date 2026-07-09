using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Procurement.Aggregates;
using CUInventory.Procurement.Interfaces;

namespace CUInventory.Procurement.Managers;

public class SupplierManager : DomainService, ISupplierManager
{
    public Task<Supplier> CreateAsync(/* TODO: parameters once Supplier has properties */)
    {
        throw new NotImplementedException();
    }
}
