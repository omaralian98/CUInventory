using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Procurement.Aggregates;

namespace CUInventory.Procurement.Interfaces;

public interface IPurchaseOrderManager : IDomainService
{
    Task<PurchaseOrder> CreateAsync(/* TODO: parameters once PurchaseOrder has properties */);
}
