using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace CUInventory.Data;

/* This is used if database provider does't define
 * ICUInventoryDbSchemaMigrator implementation.
 */
public class NullCUInventoryDbSchemaMigrator : ICUInventoryDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
