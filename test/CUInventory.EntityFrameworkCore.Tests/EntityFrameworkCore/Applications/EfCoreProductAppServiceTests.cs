using CUInventory.Catalog;
using Xunit;

namespace CUInventory.EntityFrameworkCore.Applications;

[Collection(CUInventoryTestConsts.CollectionDefinitionName)]
public class EfCoreProductAppServiceTests : ProductAppServiceTests<CUInventoryEntityFrameworkCoreTestModule>
{
}
