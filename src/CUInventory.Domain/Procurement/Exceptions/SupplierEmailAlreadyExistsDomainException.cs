using CUInventory.ValueObjects;
using Volo.Abp;

namespace CUInventory.Procurement.Exceptions;

public class SupplierEmailAlreadyExistsDomainException : BusinessException
{
    public SupplierEmailAlreadyExistsDomainException(Email email)
        : base(CUInventoryDomainErrorCodes.SupplierEmailAlreadyExists)
    {
        WithData("Email", email.Value);
    }
}
