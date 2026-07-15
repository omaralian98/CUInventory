import type { InventoryLotSource } from '../../inventory/inventory-lot-source.enum';
import type { PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface InventoryValuationItemDto {
  warehouseId?: string;
  categoryId?: string | null;
  totalQuantity?: number;
  totalValue?: number;
}

export interface InventoryValuationReportDto {
  items?: InventoryValuationItemDto[];
  totalCount?: number;
  grandTotalQuantity?: number;
  grandTotalValue?: number;
}

export interface LowStockItemDto {
  warehouseId?: string;
  productId?: string;
  quantityOnHand?: number;
  quantityReserved?: number;
  quantityAvailable?: number;
  lowStockThreshold?: number | null;
}

export interface RemainingStockDetailDto {
  lotId?: string;
  productId?: string;
  warehouseId?: string;
  supplierId?: string | null;
  shipmentLineId?: string | null;
  source?: InventoryLotSource;
  remainingQuantity?: number;
  unitCost?: number;
  valueAtCost?: number;
  receivedAt?: string;
}

export interface RemainingStockItemDto {
  warehouseId?: string;
  supplierId?: string | null;
  productId?: string;
  remainingQuantity?: number;
  valueAtCost?: number;
}

export interface RemainingStockReportDto {
  items?: RemainingStockItemDto[];
  totalCount?: number;
  totalRemainingQuantity?: number;
  totalValueAtCost?: number;
}

export interface ReportFilterInput extends PagedAndSortedResultRequestDto {
  warehouseId?: string | null;
  supplierId?: string | null;
  categoryId?: string | null;
  productId?: string | null;
  fromDate?: string | null;
  toDate?: string | null;
}

export interface SalesBySourceItemDto {
  supplierId?: string | null;
  productId?: string;
  quantitySold?: number;
  revenue?: number;
  cost?: number;
  grossMargin?: number;
}

export interface SalesBySourceReportDto {
  items?: SalesBySourceItemDto[];
  totalCount?: number;
  totalQuantitySold?: number;
  totalRevenue?: number;
  totalCost?: number;
  totalGrossMargin?: number;
}

export interface SalesSourceDetailDto {
  saleId?: string;
  saleLineId?: string;
  productId?: string;
  supplierId?: string | null;
  inventoryLotId?: string | null;
  warehouseId?: string;
  quantity?: number;
  unitPrice?: number;
  unitCost?: number;
  revenue?: number;
  cost?: number;
  grossMargin?: number;
  confirmedAt?: string | null;
}
