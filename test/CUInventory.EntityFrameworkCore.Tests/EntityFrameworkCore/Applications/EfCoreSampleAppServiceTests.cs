using CUInventory.Samples;
using Xunit;

namespace CUInventory.EntityFrameworkCore.Applications;

[Collection(CUInventoryTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<CUInventoryEntityFrameworkCoreTestModule>
{

}
