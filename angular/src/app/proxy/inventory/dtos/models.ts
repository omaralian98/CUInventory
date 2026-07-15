import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { StockTransferStatus } from '../stock-transfer-status.enum';
import type { InventoryLotSource } from '../inventory-lot-source.enum';

export interface CreateStockTransferDto {
  sourceWarehouseId: string;
  destinationWarehouseId: string;
  lines: CreateStockTransferLineDto[];
}

export interface CreateStockTransferLineDto {
  productId: string;
  quantity?: number;
}

export interface GetInventoryBalanceListDto extends PagedAndSortedResultRequestDto {
  warehouseId?: string | null;
  productId?: string | null;
  lowStockOnly?: boolean | null;
}

export interface GetInventoryLotListDto extends PagedAndSortedResultRequestDto {
  warehouseId?: string | null;
  productId?: string | null;
  supplierId?: string | null;
  hasRemaining?: boolean | null;
  availableOnly?: boolean | null;
}

export interface GetStockTransferListDto extends PagedAndSortedResultRequestDto {
  sourceWarehouseId?: string | null;
  destinationWarehouseId?: string | null;
  status?: StockTransferStatus | null;
}

export interface InventoryBalanceDto extends FullAuditedEntityDto<string> {
  warehouseId?: string;
  productId?: string;
  quantityOnHand?: number;
  quantityReserved?: number;
  quantityAvailable?: number;
  lowStockThreshold?: number | null;
  concurrencyStamp?: string;
}

export interface InventoryLotDto extends FullAuditedEntityDto<string> {
  productId?: string;
  supplierId?: string | null;
  warehouseId?: string;
  shipmentLineId?: string | null;
  source?: InventoryLotSource;
  originalQuantity?: number;
  remainingQuantity?: number;
  reservedQuantity?: number;
  availableQuantity?: number;
  unitCost?: number;
  receivedAt?: string;
}

export interface SetLowStockThresholdDto {
  threshold?: number | null;
  concurrencyStamp?: string;
}

export interface StockTransferDto extends FullAuditedEntityDto<string> {
  sourceWarehouseId?: string;
  destinationWarehouseId?: string;
  status?: StockTransferStatus;
  dispatchedAt?: string | null;
  receivedAt?: string | null;
  linesCount?: number;
  lines?: StockTransferLineDto[];
  allocations?: TransferAllocationDto[];
  concurrencyStamp?: string;
}

export interface StockTransferLineDto {
  id?: string;
  productId?: string;
  quantity?: number;
}

export interface TransferAllocationDto {
  id?: string;
  sourceLotId?: string;
  productId?: string;
  supplierId?: string | null;
  unitCost?: number;
  quantity?: number;
}
