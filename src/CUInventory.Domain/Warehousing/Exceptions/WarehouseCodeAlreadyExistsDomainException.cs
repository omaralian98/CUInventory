using Volo.Abp;

namespace CUInventory.Warehousing.Exceptions;

public class WarehouseCodeAlreadyExistsDomainException : BusinessException
{
    public WarehouseCodeAlreadyExistsDomainException(string code)
        : base(CUInventoryDomainErrorCodes.WarehouseCodeAlreadyExists)
    {
        WithData("Code", code);
    }
}
