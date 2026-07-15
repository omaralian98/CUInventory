using System;
using CUInventory.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ExceptionHandling;
using Volo.Abp.ExceptionHandling.Localization;
using Volo.Abp.Http;
using Volo.Abp.Localization.ExceptionHandling;

namespace CUInventory.ExceptionHandling;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IExceptionToErrorInfoConverter))]
public class CUInventoryExceptionToErrorInfoConverter(
    IOptions<AbpExceptionHandlingOptions> exceptionHandlingOptions,
    IOptions<AbpExceptionLocalizationOptions> localizationOptions,
    IStringLocalizerFactory stringLocalizerFactory,
    IStringLocalizer<AbpExceptionHandlingResource> stringLocalizer,
    IServiceProvider serviceProvider)
    : DefaultExceptionToErrorInfoConverter(exceptionHandlingOptions, localizationOptions, stringLocalizerFactory,
        stringLocalizer, serviceProvider)
{
    protected override RemoteServiceErrorInfo CreateErrorInfoWithoutCode(Exception exception, AbpExceptionHandlingOptions options)
    {
        var errorInfo = base.CreateErrorInfoWithoutCode(exception, options);

        if (errorInfo.Message == L["InternalServerErrorMessage"])
        {
            errorInfo.Message = StringLocalizerFactory.Create(typeof(CUInventoryResource))["InternalServerError"];
        }

        return errorInfo;
    }
}
