import type { GetInventoryBalanceListDto, InventoryBalanceDto, SetLowStockThresholdDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class InventoryBalanceService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InventoryBalanceDto>({
      method: 'GET',
      url: `/api/app/inventory-balance/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetInventoryBalanceListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<InventoryBalanceDto>>({
      method: 'GET',
      url: '/api/app/inventory-balance',
      params: { warehouseId: input.warehouseId, productId: input.productId, lowStockOnly: input.lowStockOnly, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  setLowStockThreshold = (id: string, input: SetLowStockThresholdDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InventoryBalanceDto>({
      method: 'POST',
      url: `/api/app/inventory-balance/${id}/set-low-stock-threshold`,
      body: input,
    },
    { apiName: this.apiName,...config });
}