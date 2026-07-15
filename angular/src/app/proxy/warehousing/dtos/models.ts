import type { AddressDto } from '../../shared/dtos/models';
import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ShipmentStatus } from '../shipment-status.enum';

export interface CreateShipmentDto {
  purchaseOrderId: string;
  supplierId: string;
  destinationWarehouseId: string;
  lines: CreateShipmentLineDto[];
}

export interface CreateShipmentLineDto {
  productId: string;
  quantity?: number;
  unitCost?: number;
}

export interface CreateWarehouseDto {
  name: string;
  code: string;
  address: AddressDto;
  isActive?: boolean;
  orderIndex?: number;
}

export interface GetShipmentListDto extends PagedAndSortedResultRequestDto {
  purchaseOrderId?: string | null;
  destinationWarehouseId?: string | null;
  status?: ShipmentStatus | null;
}

export interface GetWarehouseListDto extends PagedAndSortedResultRequestDto {
  filter?: string | null;
  includeInactive?: boolean;
}

export interface ShipmentDto extends FullAuditedEntityDto<string> {
  purchaseOrderId?: string;
  supplierId?: string;
  destinationWarehouseId?: string;
  status?: ShipmentStatus;
  dispatchedAt?: string | null;
  receivedAt?: string | null;
  lines?: ShipmentLineDto[];
  concurrencyStamp?: string;
}

export interface ShipmentLineDto {
  id?: string;
  productId?: string;
  quantity?: number;
  unitCost?: number;
}

export interface UpdateWarehouseDto {
  name: string;
  code: string;
  address: AddressDto;
  isActive?: boolean;
  orderIndex?: number;
  concurrencyStamp?: string;
}

export interface WarehouseDto extends FullAuditedEntityDto<string> {
  name?: string;
  code?: string;
  address?: AddressDto;
  isActive?: boolean;
  orderIndex?: number;
  concurrencyStamp?: string;
}
