using CUInventory.Sales;
using Xunit;

namespace CUInventory.EntityFrameworkCore.Applications;

[Collection(CUInventoryTestConsts.CollectionDefinitionName)]
public class EfCoreSaleAppServiceTests : SaleAppServiceTests<CUInventoryEntityFrameworkCoreTestModule>
{
}
