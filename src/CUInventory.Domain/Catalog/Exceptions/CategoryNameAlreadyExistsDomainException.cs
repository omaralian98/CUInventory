using Volo.Abp;

namespace CUInventory.Catalog.Exceptions;

public class CategoryNameAlreadyExistsDomainException : BusinessException
{
    public CategoryNameAlreadyExistsDomainException(string name)
        : base(CUInventoryDomainErrorCodes.CategoryNameAlreadyExists)
    {
        WithData("Name", name);
    }
}
