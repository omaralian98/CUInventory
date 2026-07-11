using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Inventory.Aggregates;
using CUInventory.Sales.Aggregates;

namespace CUInventory.Sales.Interfaces;

public interface ISaleManager : IDomainService
{
    Task<Sale> CreateAsync(List<SaleLineRequest> lines, List<InventoryBalance> balances, List<InventoryLot> candidateLots);

    Task<Sale> ConfirmAsync(Sale sale, List<InventoryBalance> balances, List<InventoryLot> candidateLots);

    Task<Sale> CancelAsync(Sale sale, List<InventoryBalance> balances);
}
