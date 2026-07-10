using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Procurement.Aggregates;
using CUInventory.Procurement.Exceptions;
using CUInventory.Procurement.Interfaces;
using CUInventory.Procurement.Repositories;
using CUInventory.ValueObjects;

namespace CUInventory.Procurement.Managers;

public class SupplierManager(ISupplierRepository supplierRepository) : DomainService, ISupplierManager
{
    public async Task<Supplier> CreateAsync(string name, ContactInfo contact)
    {
        var supplier = new Supplier(GuidGenerator.Create(), name, contact);
        await EnsureContactIsUniqueAsync(supplier);
        return supplier;
    }

    public async Task<Supplier> UpdateAsync(Supplier supplier, string name, ContactInfo contact)
    {
        supplier.SetName(name);

        if (supplier.Contact != contact)
        {
            supplier.SetContact(contact);
            await EnsureContactIsUniqueAsync(supplier);
        }

        return supplier;
    }

    private async Task EnsureContactIsUniqueAsync(Supplier supplier)
    {
        var existingByEmail = await supplierRepository.GetByEmailOrDefaultAsync(supplier.Contact.Email);
        if (existingByEmail is not null && existingByEmail.Id != supplier.Id)
        {
            throw new SupplierEmailAlreadyExistsDomainException(supplier.Contact.Email);
        }

        var existingByPhone = await supplierRepository.GetByPhoneNumberOrDefaultAsync(supplier.Contact.PhoneNumber);
        if (existingByPhone is not null && existingByPhone.Id != supplier.Id)
        {
            throw new SupplierPhoneNumberAlreadyExistsDomainException(supplier.Contact.PhoneNumber);
        }
    }
}
