import type { CreateWarehouseDto, GetWarehouseListDto, UpdateWarehouseDto, WarehouseDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class WarehouseService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  create = (input: CreateWarehouseDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarehouseDto>({
      method: 'POST',
      url: '/api/app/warehouse',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/warehouse/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarehouseDto>({
      method: 'GET',
      url: `/api/app/warehouse/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetWarehouseListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<WarehouseDto>>({
      method: 'GET',
      url: '/api/app/warehouse',
      params: { filter: input.filter, includeInactive: input.includeInactive, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdateWarehouseDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarehouseDto>({
      method: 'PUT',
      url: `/api/app/warehouse/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}