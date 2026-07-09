using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Warehousing.Aggregates;

namespace CUInventory.Warehousing.Interfaces;

public interface IShipmentManager : IDomainService
{
    Task<Shipment> CreateAsync(/* TODO: parameters once Shipment has properties */);
}
