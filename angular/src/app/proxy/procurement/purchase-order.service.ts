import type { CreatePurchaseOrderDto, GetPurchaseOrderListDto, PurchaseOrderDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import type { ConcurrencyStampDto } from '../shared/dtos/models';

@Injectable({
  providedIn: 'root',
})
export class PurchaseOrderService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  cancel = (id: string, input: ConcurrencyStampDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseOrderDto>({
      method: 'POST',
      url: `/api/app/purchase-order/${id}/cancel`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  confirm = (id: string, input: ConcurrencyStampDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseOrderDto>({
      method: 'POST',
      url: `/api/app/purchase-order/${id}/confirm`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreatePurchaseOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseOrderDto>({
      method: 'POST',
      url: '/api/app/purchase-order',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/purchase-order/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseOrderDto>({
      method: 'GET',
      url: `/api/app/purchase-order/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetPurchaseOrderListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PurchaseOrderDto>>({
      method: 'GET',
      url: '/api/app/purchase-order',
      params: { filter: input.filter, supplierId: input.supplierId, destinationWarehouseId: input.destinationWarehouseId, status: input.status, statuses: input.statuses, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
}