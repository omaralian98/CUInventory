using CUInventory.Procurement;
using Xunit;

namespace CUInventory.EntityFrameworkCore.Applications;

[Collection(CUInventoryTestConsts.CollectionDefinitionName)]
public class EfCorePurchaseOrderAppServiceTests : PurchaseOrderAppServiceTests<CUInventoryEntityFrameworkCoreTestModule>
{
}
