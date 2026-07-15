import type { AllocationStrategyKind } from '../../inventory/allocation-strategy-kind.enum';
import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { SaleStatus } from '../sale-status.enum';

export interface CreateSaleDto {
  lines: CreateSaleLineDto[];
}

export interface CreateSaleLineDto {
  productId: string;
  quantity?: number;
  unitPrice?: number;
  kind?: AllocationStrategyKind;
  warehouseId?: string | null;
  supplierId?: string | null;
  lotId?: string | null;
}

export interface GetSaleListDto extends PagedAndSortedResultRequestDto {
  status?: SaleStatus | null;
}

export interface SaleAllocationDto {
  id?: string;
  warehouseId?: string;
  inventoryLotId?: string | null;
  supplierId?: string | null;
  quantity?: number;
  unitCost?: number | null;
  isReserved?: boolean;
}

export interface SaleDto extends FullAuditedEntityDto<string> {
  status?: SaleStatus;
  confirmedAt?: string | null;
  linesCount?: number;
  totalAmount?: number;
  lines?: SaleLineDto[];
  concurrencyStamp?: string;
}

export interface SaleLineDto {
  id?: string;
  productId?: string;
  quantity?: number;
  unitPrice?: number;
  kind?: AllocationStrategyKind;
  warehouseId?: string | null;
  supplierId?: string | null;
  lotId?: string | null;
  allocations?: SaleAllocationDto[];
}
