import type { AddressDto } from '../../shared/dtos/models';
import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { PurchaseOrderStatus } from '../purchase-order-status.enum';

export interface ContactInfoDto {
  email: string;
  phoneNumber: string;
  address: AddressDto;
}

export interface CreatePurchaseOrderDto {
  supplierId: string;
  destinationWarehouseId: string;
  lines: CreatePurchaseOrderLineDto[];
}

export interface CreatePurchaseOrderLineDto {
  productId: string;
  orderedQuantity?: number;
  unitCost?: number;
}

export interface CreateSupplierDto {
  name: string;
  contact: ContactInfoDto;
}

export interface GetPurchaseOrderListDto extends PagedAndSortedResultRequestDto {
  filter?: string | null;
  supplierId?: string | null;
  destinationWarehouseId?: string | null;
  status?: PurchaseOrderStatus | null;
  statuses?: PurchaseOrderStatus[] | null;
}

export interface GetSupplierListDto extends PagedAndSortedResultRequestDto {
  filter?: string | null;
}

export interface PurchaseOrderDto extends FullAuditedEntityDto<string> {
  supplierId?: string;
  destinationWarehouseId?: string;
  status?: PurchaseOrderStatus;
  linesCount?: number;
  lines?: PurchaseOrderLineDto[];
  concurrencyStamp?: string;
}

export interface PurchaseOrderLineDto {
  id?: string;
  productId?: string;
  orderedQuantity?: number;
  receivedQuantity?: number;
  unitCost?: number;
  outstandingQuantity?: number;
  isFullyReceived?: boolean;
}

export interface SupplierDto extends FullAuditedEntityDto<string> {
  name?: string;
  contact?: ContactInfoDto;
  concurrencyStamp?: string;
}

export interface UpdateSupplierDto {
  name: string;
  contact: ContactInfoDto;
  concurrencyStamp?: string;
}
