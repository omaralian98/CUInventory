using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Procurement.Aggregates;
using CUInventory.Procurement.Interfaces;

namespace CUInventory.Procurement.Managers;

public class PurchaseOrderManager : DomainService, IPurchaseOrderManager
{
    public Task<PurchaseOrder> CreateAsync(/* TODO: parameters once PurchaseOrder has properties */)
    {
        throw new NotImplementedException();
    }
}
