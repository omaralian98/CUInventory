using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using CUInventory.Procurement.Aggregates;
using CUInventory.ValueObjects;

namespace CUInventory.Procurement.Repositories;

public interface ISupplierRepository : IRepository<Supplier, Guid>
{
    Task<Supplier?> GetByEmailOrDefaultAsync(Email email);
    Task<Supplier?> GetByPhoneNumberOrDefaultAsync(PhoneNumber phoneNumber);
}
