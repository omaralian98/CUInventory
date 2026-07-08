using CUInventory.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace CUInventory.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class CUInventoryController : AbpControllerBase
{
    protected CUInventoryController()
    {
        LocalizationResource = typeof(CUInventoryResource);
    }
}
