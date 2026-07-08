using Volo.Abp.Modularity;

namespace CUInventory;

[DependsOn(
    typeof(CUInventoryDomainModule),
    typeof(CUInventoryTestBaseModule)
)]
public class CUInventoryDomainTestModule : AbpModule
{

}
