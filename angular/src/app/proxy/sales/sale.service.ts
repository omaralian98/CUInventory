import type { CreateSaleDto, GetSaleListDto, SaleDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import type { ConcurrencyStampDto } from '../shared/dtos/models';

@Injectable({
  providedIn: 'root',
})
export class SaleService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  cancel = (id: string, input: ConcurrencyStampDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SaleDto>({
      method: 'POST',
      url: `/api/app/sale/${id}/cancel`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  confirm = (id: string, input: ConcurrencyStampDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SaleDto>({
      method: 'POST',
      url: `/api/app/sale/${id}/confirm`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateSaleDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SaleDto>({
      method: 'POST',
      url: '/api/app/sale',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/sale/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SaleDto>({
      method: 'GET',
      url: `/api/app/sale/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetSaleListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<SaleDto>>({
      method: 'GET',
      url: '/api/app/sale',
      params: { status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
}