using Volo.Abp;

namespace CUInventory.Common.Exceptions;

public class ArgumentMustBePositiveDomainException : BusinessException
{
    public ArgumentMustBePositiveDomainException(string parameterName, decimal value)
        : base(CUInventoryDomainErrorCodes.ArgumentMustBePositive)
    {
        WithData("ParameterName", parameterName);
        WithData("Value", value);
    }
}
