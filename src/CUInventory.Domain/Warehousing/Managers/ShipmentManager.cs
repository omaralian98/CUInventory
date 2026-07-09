using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Warehousing.Aggregates;
using CUInventory.Warehousing.Interfaces;

namespace CUInventory.Warehousing.Managers;

public class ShipmentManager : DomainService, IShipmentManager
{
    public Task<Shipment> CreateAsync(/* TODO: parameters once Shipment has properties */)
    {
        throw new NotImplementedException();
    }
}
