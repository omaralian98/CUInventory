using Volo.Abp.Modularity;

namespace CUInventory;

[DependsOn(
    typeof(CUInventoryApplicationModule),
    typeof(CUInventoryDomainTestModule)
)]
public class CUInventoryApplicationTestModule : AbpModule
{

}
