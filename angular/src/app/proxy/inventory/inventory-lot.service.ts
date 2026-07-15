import type { GetInventoryLotListDto, InventoryLotDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class InventoryLotService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InventoryLotDto>({
      method: 'GET',
      url: `/api/app/inventory-lot/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetInventoryLotListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<InventoryLotDto>>({
      method: 'GET',
      url: '/api/app/inventory-lot',
      params: { warehouseId: input.warehouseId, productId: input.productId, supplierId: input.supplierId, hasRemaining: input.hasRemaining, availableOnly: input.availableOnly, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
}