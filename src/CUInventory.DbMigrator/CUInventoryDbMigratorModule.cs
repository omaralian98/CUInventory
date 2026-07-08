using CUInventory.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace CUInventory.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(CUInventoryEntityFrameworkCoreModule),
    typeof(CUInventoryApplicationContractsModule)
)]
public class CUInventoryDbMigratorModule : AbpModule
{
}
