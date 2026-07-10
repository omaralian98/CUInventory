using CUInventory.ValueObjects;
using Volo.Abp;

namespace CUInventory.Procurement.Exceptions;

public class SupplierPhoneNumberAlreadyExistsDomainException : BusinessException
{
    public SupplierPhoneNumberAlreadyExistsDomainException(PhoneNumber phoneNumber)
        : base(CUInventoryDomainErrorCodes.SupplierPhoneNumberAlreadyExists)
    {
        WithData("PhoneNumber", phoneNumber.Value);
    }
}
