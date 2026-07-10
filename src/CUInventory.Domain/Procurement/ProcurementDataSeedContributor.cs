using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CUInventory.Procurement.Interfaces;
using CUInventory.Procurement.Repositories;
using CUInventory.ValueObjects;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace CUInventory.Procurement;

public class ProcurementDataSeedContributor(
    ISupplierManager supplierManager,
    ISupplierRepository supplierRepository)
    : IDataSeedContributor, ITransientDependency
{
    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        var suppliers = new List<(string Name, string Email, string Phone, string Governorate, string City, string Street)>
        {
            ("Globex Electronics", "sales@globex.example", "+963 933 405 934", "Damascus", "Damascus", "Al-Mazzeh St"),
            ("Initech Office Supplies", "orders@initech.example", "+963 983 830 510", "Aleppo", "Aleppo", "Al-Furqan St"),
            ("Umbrella Furniture Co.", "contact@umbrella.example", "+963 331 458 786", "Homs", "Homs", "Al-Waer St"),
        };

        foreach (var supplier in suppliers)
        {
            try
            {
                await SeedSupplierAsync(supplier.Name, supplier.Email, supplier.Phone,
                    supplier.Governorate, supplier.City, supplier.Street);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private async Task SeedSupplierAsync(
        string name, string email, string phone, string governorate, string city, string street)
    {
        var existing = await supplierRepository.FindAsync(s => s.Name == name);
        if (existing is not null)
        {
            return;
        }

        var contact = new ContactInfo(
            new Email(email),
            new PhoneNumber(phone),
            new Address(governorate, city, street));

        var supplier = await supplierManager.CreateAsync(name, contact);
        await supplierRepository.InsertAsync(supplier, autoSave: true);
    }
}
