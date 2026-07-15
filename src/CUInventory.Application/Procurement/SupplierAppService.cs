using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Permissions;
using CUInventory.Procurement.Aggregates;
using CUInventory.Procurement.Dtos;
using CUInventory.Procurement.Interfaces;
using CUInventory.Procurement.Repositories;
using CUInventory.ValueObjects;
using Microsoft.AspNetCore.Authorization;

namespace CUInventory.Procurement;

[Authorize(CUInventoryPermissions.Suppliers.Default)]
public class SupplierAppService :
    CUInventoryCrudAppService<Supplier, SupplierDto, SupplierDto, Guid, GetSupplierListDto, CreateSupplierDto, UpdateSupplierDto>,
    ISupplierAppService
{
    private readonly ISupplierManager _supplierManager;

    public SupplierAppService(ISupplierRepository repository, ISupplierManager supplierManager)
        : base(repository)
    {
        _supplierManager = supplierManager;

        GetPolicyName = CUInventoryPermissions.Suppliers.Default;
        GetListPolicyName = CUInventoryPermissions.Suppliers.Default;
        CreatePolicyName = CUInventoryPermissions.Suppliers.Create;
        UpdatePolicyName = CUInventoryPermissions.Suppliers.Edit;
        DeletePolicyName = CUInventoryPermissions.Suppliers.Delete;
    }

    public override async Task<SupplierDto> CreateAsync(CreateSupplierDto input)
    {
        await CheckCreatePolicyAsync();

        var supplier = await _supplierManager.CreateAsync(input.Name, ToContactInfo(input.Contact));

        await Repository.InsertAsync(supplier, autoSave: true);
        return await MapToGetOutputDtoAsync(supplier);
    }

    public override async Task<SupplierDto> UpdateAsync(Guid id, UpdateSupplierDto input)
    {
        await CheckUpdatePolicyAsync();

        var supplier = await Repository.GetAsync(id);
        supplier.ConcurrencyStamp = input.ConcurrencyStamp;
        await _supplierManager.UpdateAsync(supplier, input.Name, ToContactInfo(input.Contact));

        await Repository.UpdateAsync(supplier, autoSave: true);
        return await MapToGetOutputDtoAsync(supplier);
    }

    protected override async Task<IQueryable<Supplier>> CreateFilteredQueryAsync(GetSupplierListDto input)
    {
        var query = await Repository.GetQueryableAsync();
        return query.WhereIf(
            !string.IsNullOrWhiteSpace(input.Filter),
            s => s.Name.Contains(input.Filter!) || s.Contact.Email.Value.Contains(input.Filter!));
    }

    private static ContactInfo ToContactInfo(ContactInfoDto dto) =>
        new(
            new Email(dto.Email),
            new PhoneNumber(dto.PhoneNumber),
            new Address(dto.Address.Governorate, dto.Address.City, dto.Address.Street));
}
