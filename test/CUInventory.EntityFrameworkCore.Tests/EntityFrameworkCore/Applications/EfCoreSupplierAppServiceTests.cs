using CUInventory.Procurement;
using Xunit;

namespace CUInventory.EntityFrameworkCore.Applications;

[Collection(CUInventoryTestConsts.CollectionDefinitionName)]
public class EfCoreSupplierAppServiceTests : SupplierAppServiceTests<CUInventoryEntityFrameworkCoreTestModule>
{
}
