using Microsoft.Extensions.Localization;
using CUInventory.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace CUInventory;

[Dependency(ReplaceServices = true)]
public class CUInventoryBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<CUInventoryResource> _localizer;

    public CUInventoryBrandingProvider(IStringLocalizer<CUInventoryResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
