using CUInventory.Inventory;
using Xunit;

namespace CUInventory.EntityFrameworkCore.Applications;

[Collection(CUInventoryTestConsts.CollectionDefinitionName)]
public class EfCoreInventoryLotAppServiceTests : InventoryLotAppServiceTests<CUInventoryEntityFrameworkCoreTestModule>
{
}
