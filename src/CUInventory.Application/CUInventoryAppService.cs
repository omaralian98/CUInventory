using CUInventory.Localization;
using Volo.Abp.Application.Services;

namespace CUInventory;

/* Inherit your application services from this class.
 */
public abstract class CUInventoryAppService : ApplicationService
{
    protected CUInventoryAppService()
    {
        LocalizationResource = typeof(CUInventoryResource);
    }
}
