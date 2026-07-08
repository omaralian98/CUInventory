using Volo.Abp.Modularity;

namespace CUInventory;

public abstract class CUInventoryApplicationTestBase<TStartupModule> : CUInventoryTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
