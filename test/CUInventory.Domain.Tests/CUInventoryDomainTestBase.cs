using Volo.Abp.Modularity;

namespace CUInventory;

/* Inherit from this class for your domain layer tests. */
public abstract class CUInventoryDomainTestBase<TStartupModule> : CUInventoryTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
