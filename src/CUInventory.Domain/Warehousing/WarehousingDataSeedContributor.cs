using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CUInventory.ValueObjects;
using CUInventory.Warehousing.Aggregates;
using CUInventory.Warehousing.Interfaces;
using CUInventory.Warehousing.Repositories;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace CUInventory.Warehousing;

public class WarehousingDataSeedContributor(
    IWarehouseManager warehouseManager,
    IWarehouseRepository warehouseRepository)
    : IDataSeedContributor, ITransientDependency
{
    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        var warehouses = new List<(string Name, string Code, string Governorate, string City, string Street)>
        {
            ("Damascus Central Warehouse", "WH-DAM", "Damascus", "Damascus", "Industrial City Rd"),
            ("Aleppo North Warehouse", "WH-ALE", "Aleppo", "Aleppo", "Sheikh Najjar Rd"),
            ("Homs Distribution Center", "WH-HOM", "Homs", "Homs", "Al-Waer St"),
            ("Hama Regional Warehouse", "WH-HAM", "Hama", "Hama", "Aleppo Rd"),
            ("Latakia Port Warehouse", "WH-LAT", "Latakia", "Latakia", "Port Rd"),
            ("Tartus Coastal Warehouse", "WH-TAR", "Tartus", "Tartus", "Corniche St"),
            ("Deir ez-Zor Warehouse", "WH-DEZ", "Deir ez-Zor", "Deir ez-Zor", "Euphrates St"),
            ("Raqqa Warehouse", "WH-RAQ", "Raqqa", "Raqqa", "Al-Mansour St"),
            ("Hasakah Warehouse", "WH-HAS", "Hasakah", "Hasakah", "Al-Aziziyah St"),
            ("Daraa South Warehouse", "WH-DAR", "Daraa", "Daraa", "Al-Sad Rd"),
        };

        foreach (var warehouse in warehouses)
        {
            try
            {
                await SeedWarehouseAsync(
                    warehouse.Name,
                    warehouse.Code,
                    warehouse.Governorate,
                    warehouse.City,
                    warehouse.Street);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private async Task SeedWarehouseAsync(
        string name, string code, string governorate, string city, string street)
    {
        var normalizedCode = Warehouse.NormalizeCode(code);
        var existing = await warehouseRepository.GetByCodeOrDefaultAsync(normalizedCode);
        if (existing is not null)
        {
            return;
        }

        var address = new Address(governorate, city, street);

        var warehouse = await warehouseManager.CreateAsync(name, code, address);
        await warehouseRepository.InsertAsync(warehouse, autoSave: true);
    }
}
