using System;
using System.Threading.Tasks;
using CUInventory.Procurement.Aggregates;
using CUInventory.Procurement.Repositories;
using CUInventory.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace CUInventory.EntityFrameworkCore.Repositories.Procurement;

public class EfCoreSupplierRepository(IDbContextProvider<CUInventoryDbContext> dbContextProvider)
    : EfCoreRepository<CUInventoryDbContext, Supplier, Guid>(dbContextProvider), ISupplierRepository
{
    public async Task<Supplier?> GetByEmailOrDefaultAsync(Email email)
    {
        var query = await WithDetailsAsync();
        return await query.FirstOrDefaultAsync(
            x => x.Contact.Email.Value == email.Value, GetCancellationToken());
    }

    public async Task<Supplier?> GetByPhoneNumberOrDefaultAsync(PhoneNumber phoneNumber)
    {
        var query = await WithDetailsAsync();
        return await query.FirstOrDefaultAsync(
            x => x.Contact.PhoneNumber.Value == phoneNumber.Value, GetCancellationToken());
    }
}
