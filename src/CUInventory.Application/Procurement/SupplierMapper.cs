using CUInventory.Procurement.Aggregates;
using CUInventory.Procurement.Dtos;
using CUInventory.ValueObjects;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace CUInventory.Procurement;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SupplierMapper : MapperBase<Supplier, SupplierDto>
{
    public override partial SupplierDto Map(Supplier source);

    public override partial void Map(Supplier source, SupplierDto destination);

    private static string MapEmail(Email email) => email.Value;

    private static string MapPhoneNumber(PhoneNumber phoneNumber) => phoneNumber.Value;
}
