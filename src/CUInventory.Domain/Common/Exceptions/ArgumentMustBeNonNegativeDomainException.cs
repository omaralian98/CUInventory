using Volo.Abp;

namespace CUInventory.Common.Exceptions;

public class ArgumentMustBeNonNegativeDomainException : BusinessException
{
    public ArgumentMustBeNonNegativeDomainException(string parameterName, decimal value)
        : base(CUInventoryDomainErrorCodes.ArgumentMustBeNonNegative)
    {
        WithData("ParameterName", parameterName);
        WithData("Value", value);
    }
}
