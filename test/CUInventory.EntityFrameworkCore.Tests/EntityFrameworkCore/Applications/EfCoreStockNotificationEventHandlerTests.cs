using CUInventory.Inventory.RealTime;
using Xunit;

namespace CUInventory.EntityFrameworkCore.Applications;

[Collection(CUInventoryTestConsts.CollectionDefinitionName)]
public class EfCoreStockNotificationEventHandlerTests : StockNotificationEventHandlerTests<CUInventoryEntityFrameworkCoreTestModule>
{
}
