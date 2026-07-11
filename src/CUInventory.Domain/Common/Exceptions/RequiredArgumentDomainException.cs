using Volo.Abp;

namespace CUInventory.Common.Exceptions;

public class RequiredArgumentDomainException : BusinessException
{
    public RequiredArgumentDomainException(string parameterName)
        : base(CUInventoryDomainErrorCodes.RequiredArgument)
    {
        WithData("ParameterName", parameterName);
    }
}
