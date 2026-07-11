using Volo.Abp;

namespace CUInventory.Common.Exceptions;

public class ArgumentInvalidFormatDomainException : BusinessException
{
    public ArgumentInvalidFormatDomainException(string parameterName, string value)
        : base(CUInventoryDomainErrorCodes.ArgumentInvalidFormat)
    {
        WithData("ParameterName", parameterName);
        WithData("Value", value);
    }
}
