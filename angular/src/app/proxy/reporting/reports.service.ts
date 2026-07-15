import type { InventoryValuationReportDto, LowStockItemDto, RemainingStockDetailDto, RemainingStockReportDto, ReportFilterInput, SalesBySourceReportDto, SalesSourceDetailDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ReportsService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  getInventoryValuation = (input: ReportFilterInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InventoryValuationReportDto>({
      method: 'GET',
      url: '/api/app/reports/inventory-valuation',
      params: { warehouseId: input.warehouseId, supplierId: input.supplierId, categoryId: input.categoryId, productId: input.productId, fromDate: input.fromDate, toDate: input.toDate, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getLowStock = (input: ReportFilterInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<LowStockItemDto>>({
      method: 'GET',
      url: '/api/app/reports/low-stock',
      params: { warehouseId: input.warehouseId, supplierId: input.supplierId, categoryId: input.categoryId, productId: input.productId, fromDate: input.fromDate, toDate: input.toDate, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getRemainingStock = (input: ReportFilterInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, RemainingStockReportDto>({
      method: 'GET',
      url: '/api/app/reports/remaining-stock',
      params: { warehouseId: input.warehouseId, supplierId: input.supplierId, categoryId: input.categoryId, productId: input.productId, fromDate: input.fromDate, toDate: input.toDate, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getRemainingStockDetail = (input: ReportFilterInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<RemainingStockDetailDto>>({
      method: 'GET',
      url: '/api/app/reports/remaining-stock-detail',
      params: { warehouseId: input.warehouseId, supplierId: input.supplierId, categoryId: input.categoryId, productId: input.productId, fromDate: input.fromDate, toDate: input.toDate, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getSalesBySource = (input: ReportFilterInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SalesBySourceReportDto>({
      method: 'GET',
      url: '/api/app/reports/sales-by-source',
      params: { warehouseId: input.warehouseId, supplierId: input.supplierId, categoryId: input.categoryId, productId: input.productId, fromDate: input.fromDate, toDate: input.toDate, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getSalesBySourceDetail = (input: ReportFilterInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<SalesSourceDetailDto>>({
      method: 'GET',
      url: '/api/app/reports/sales-by-source-detail',
      params: { warehouseId: input.warehouseId, supplierId: input.supplierId, categoryId: input.categoryId, productId: input.productId, fromDate: input.fromDate, toDate: input.toDate, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
}