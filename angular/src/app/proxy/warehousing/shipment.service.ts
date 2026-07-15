import type { CreateShipmentDto, GetShipmentListDto, ShipmentDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import type { ConcurrencyStampDto } from '../shared/dtos/models';

@Injectable({
  providedIn: 'root',
})
export class ShipmentService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  create = (input: CreateShipmentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShipmentDto>({
      method: 'POST',
      url: '/api/app/shipment',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/shipment/${id}`,
    },
    { apiName: this.apiName,...config });
  

  dispatch = (id: string, input: ConcurrencyStampDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShipmentDto>({
      method: 'POST',
      url: `/api/app/shipment/${id}/dispatch`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShipmentDto>({
      method: 'GET',
      url: `/api/app/shipment/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetShipmentListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShipmentDto>>({
      method: 'GET',
      url: '/api/app/shipment',
      params: { purchaseOrderId: input.purchaseOrderId, destinationWarehouseId: input.destinationWarehouseId, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  receive = (id: string, input: ConcurrencyStampDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShipmentDto>({
      method: 'POST',
      url: `/api/app/shipment/${id}/receive`,
      body: input,
    },
    { apiName: this.apiName,...config });
}