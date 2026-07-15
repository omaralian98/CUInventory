import type { CreateStockTransferDto, GetStockTransferListDto, StockTransferDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import type { ConcurrencyStampDto } from '../shared/dtos/models';

@Injectable({
  providedIn: 'root',
})
export class StockTransferService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  cancel = (id: string, input: ConcurrencyStampDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StockTransferDto>({
      method: 'POST',
      url: `/api/app/stock-transfer/${id}/cancel`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateStockTransferDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StockTransferDto>({
      method: 'POST',
      url: '/api/app/stock-transfer',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/stock-transfer/${id}`,
    },
    { apiName: this.apiName,...config });
  

  dispatch = (id: string, input: ConcurrencyStampDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StockTransferDto>({
      method: 'POST',
      url: `/api/app/stock-transfer/${id}/dispatch`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StockTransferDto>({
      method: 'GET',
      url: `/api/app/stock-transfer/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetStockTransferListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StockTransferDto>>({
      method: 'GET',
      url: '/api/app/stock-transfer',
      params: { sourceWarehouseId: input.sourceWarehouseId, destinationWarehouseId: input.destinationWarehouseId, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  receive = (id: string, input: ConcurrencyStampDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StockTransferDto>({
      method: 'POST',
      url: `/api/app/stock-transfer/${id}/receive`,
      body: input,
    },
    { apiName: this.apiName,...config });
}