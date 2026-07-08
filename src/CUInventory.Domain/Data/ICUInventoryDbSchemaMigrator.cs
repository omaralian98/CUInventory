using System.Threading.Tasks;

namespace CUInventory.Data;

public interface ICUInventoryDbSchemaMigrator
{
    Task MigrateAsync();
}
