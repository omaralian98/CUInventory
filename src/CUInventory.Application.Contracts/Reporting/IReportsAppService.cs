using System.Threading.Tasks;
using CUInventory.Reporting.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace CUInventory.Reporting;

public interface IReportsAppService : IApplicationService
{
    Task<SalesBySourceReportDto> GetSalesBySourceAsync(ReportFilterInput input);

    Task<PagedResultDto<SalesSourceDetailDto>> GetSalesBySourceDetailAsync(ReportFilterInput input);

    Task<RemainingStockReportDto> GetRemainingStockAsync(ReportFilterInput input);

    Task<PagedResultDto<RemainingStockDetailDto>> GetRemainingStockDetailAsync(ReportFilterInput input);

    Task<InventoryValuationReportDto> GetInventoryValuationAsync(ReportFilterInput input);

    Task<PagedResultDto<LowStockItemDto>> GetLowStockAsync(ReportFilterInput input);
}
