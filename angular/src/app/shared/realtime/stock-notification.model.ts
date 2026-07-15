// Mirror of the backend CUInventory.Inventory.RealTime contracts. These are NOT in the
// generated proxy (the generated stream() stub is unusable), so we author them by hand.

export enum StockNotificationType {
  StockChanged = 0,
  LowStockReached = 1,
}

/** Server-sent event name is the StockNotificationType string. */
export type StockNotificationEventName = 'StockChanged' | 'LowStockReached';

export interface StockNotificationDto {
  type: StockNotificationType;
  inventoryBalanceId: string;
  warehouseId: string;
  productId: string;
  quantityOnHand: number;
  quantityReserved: number;
  quantityAvailable: number;
  lowStockThreshold?: number | null;
  isBelowThreshold: boolean;
  occurredAt: string;
  tenantId?: string | null;
}
