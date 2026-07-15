using CUInventory.Reporting;
using Xunit;

namespace CUInventory.EntityFrameworkCore.Applications;

[Collection(CUInventoryTestConsts.CollectionDefinitionName)]
public class EfCoreReportsAppServiceTests : ReportsAppServiceTests<CUInventoryEntityFrameworkCoreTestModule>
{
}
