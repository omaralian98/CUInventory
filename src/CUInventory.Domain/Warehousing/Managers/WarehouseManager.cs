using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.ValueObjects;
using CUInventory.Warehousing.Aggregates;
using CUInventory.Warehousing.Exceptions;
using CUInventory.Warehousing.Interfaces;
using CUInventory.Warehousing.Repositories;

namespace CUInventory.Warehousing.Managers;

public class WarehouseManager(IWarehouseRepository warehouseRepository) : DomainService, IWarehouseManager
{
    public async Task<Warehouse> CreateAsync(string name, string code, Address address)
    {
        var warehouse = new Warehouse(GuidGenerator.Create(), name, code, address);
        await EnsureCodeIsUniqueAsync(warehouse.Code, warehouse.Id);
        return warehouse;
    }

    public async Task<Warehouse> UpdateAsync(Warehouse warehouse, string name, string code, Address address, bool isActive)
    {
        var normalizedCode = Warehouse.NormalizeCode(code);
        if (string.Equals(warehouse.Code, normalizedCode, StringComparison.Ordinal) == false)
        {
            await EnsureCodeIsUniqueAsync(normalizedCode, warehouse.Id);
            warehouse.SetCode(code);
        }

        warehouse.SetName(name);
        warehouse.SetAddress(address);
        warehouse.SetIsActive(isActive);
        return warehouse;
    }

    private async Task EnsureCodeIsUniqueAsync(string code, Guid warehouseId)
    {
        var existing = await warehouseRepository.GetByCodeOrDefaultAsync(code);
        if (existing is not null && existing.Id != warehouseId)
        {
            throw new WarehouseCodeAlreadyExistsDomainException(code);
        }
    }
}
