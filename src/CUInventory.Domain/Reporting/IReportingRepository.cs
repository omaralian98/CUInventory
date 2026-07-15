using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CUInventory.Reporting;

public interface IReportingRepository
{
    Task<SalesBySourceReport> GetSalesBySourceAsync(ReportQueryFilter filter, CancellationToken cancellationToken = default);

    Task<ReportPage<SalesBySourceDetailRow>> GetSalesBySourceDetailAsync(ReportQueryFilter filter, CancellationToken cancellationToken = default);

    Task<RemainingStockReport> GetRemainingStockAsync(ReportQueryFilter filter, CancellationToken cancellationToken = default);

    Task<ReportPage<RemainingStockDetailRow>> GetRemainingStockDetailAsync(ReportQueryFilter filter, CancellationToken cancellationToken = default);

    Task<InventoryValuationReport> GetInventoryValuationAsync(ReportQueryFilter filter, CancellationToken cancellationToken = default);

    Task<ReportPage<LowStockRow>> GetLowStockAsync(ReportQueryFilter filter, CancellationToken cancellationToken = default);
}
