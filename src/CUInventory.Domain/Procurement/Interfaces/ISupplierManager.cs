using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Procurement.Aggregates;
using CUInventory.ValueObjects;

namespace CUInventory.Procurement.Interfaces;

public interface ISupplierManager : IDomainService
{
    Task<Supplier> CreateAsync(string name, ContactInfo contact);
    Task<Supplier> UpdateAsync(Supplier supplier, string name, ContactInfo contact);
}
