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
            ("Umbrella Furniture Co.", "contact@umbrella.example", "+963 964 617 208", "Homs", "Homs", "Al-Waer St"),
            ("Soylent Hardware", "sales@soylent.example", "+963 944 112 233", "Hama", "Hama", "Aleppo Rd"),
            ("Hooli Cleaning Supplies", "orders@hooli.example", "+963 955 224 466", "Latakia", "Latakia", "Port Rd"),
            ("Wonka Kitchen Supplies", "hello@wonka.example", "+963 966 337 788", "Tartus", "Tartus", "Corniche St"),
            ("Stark Safety Equipment", "sales@stark.example", "+963 934 560 812", "Damascus", "Damascus", "Baramkeh St"),
            ("Wayne Networking", "orders@wayne.example", "+963 988 556 011", "Aleppo", "Aleppo", "University St"),
            ("Acme Paper & Printing", "sales@acme.example", "+963 959 673 240", "Homs", "Homs", "Al-Hamra St"),
            ("Cyberdyne Storage Systems", "contact@cyberdyne.example", "+963 932 778 233", "Latakia", "Latakia", "Al-Ziraa St"),
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
