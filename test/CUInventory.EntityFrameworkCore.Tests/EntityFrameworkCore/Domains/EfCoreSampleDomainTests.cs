using CUInventory.Samples;
using Xunit;

namespace CUInventory.EntityFrameworkCore.Domains;

[Collection(CUInventoryTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<CUInventoryEntityFrameworkCoreTestModule>
{

}
