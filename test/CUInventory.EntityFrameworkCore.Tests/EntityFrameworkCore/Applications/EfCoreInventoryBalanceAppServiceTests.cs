using CUInventory.Inventory;
using Xunit;

namespace CUInventory.EntityFrameworkCore.Applications;

[Collection(CUInventoryTestConsts.CollectionDefinitionName)]
public class EfCoreInventoryBalanceAppServiceTests : InventoryBalanceAppServiceTests<CUInventoryEntityFrameworkCoreTestModule>
{
}
