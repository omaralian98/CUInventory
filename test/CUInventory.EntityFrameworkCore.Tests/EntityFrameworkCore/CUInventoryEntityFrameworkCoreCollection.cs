using Xunit;

namespace CUInventory.EntityFrameworkCore;

[CollectionDefinition(CUInventoryTestConsts.CollectionDefinitionName)]
public class CUInventoryEntityFrameworkCoreCollection : ICollectionFixture<CUInventoryEntityFrameworkCoreFixture>
{

}
