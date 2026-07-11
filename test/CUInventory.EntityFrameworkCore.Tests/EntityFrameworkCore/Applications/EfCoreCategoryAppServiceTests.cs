using CUInventory.Catalog;
using Xunit;

namespace CUInventory.EntityFrameworkCore.Applications;

[Collection(CUInventoryTestConsts.CollectionDefinitionName)]
public class EfCoreCategoryAppServiceTests : CategoryAppServiceTests<CUInventoryEntityFrameworkCoreTestModule>
{
}
