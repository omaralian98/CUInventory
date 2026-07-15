using CUInventory.Inventory.RealTime;
using Xunit;

namespace CUInventory.EntityFrameworkCore.Applications;

[Collection(CUInventoryTestConsts.CollectionDefinitionName)]
public class EfCoreStockNotificationTests : StockNotificationTests<CUInventoryEntityFrameworkCoreTestModule>
{
}
