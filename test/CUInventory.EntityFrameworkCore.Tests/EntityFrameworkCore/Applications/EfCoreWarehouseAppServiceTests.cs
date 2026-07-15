using CUInventory.Warehousing;
using Xunit;

namespace CUInventory.EntityFrameworkCore.Applications;

[Collection(CUInventoryTestConsts.CollectionDefinitionName)]
public class EfCoreWarehouseAppServiceTests : WarehouseAppServiceTests<CUInventoryEntityFrameworkCoreTestModule>
{
}
