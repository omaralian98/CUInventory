import { mapEnumToOptions } from '@abp/ng.core';

export enum PurchaseOrderStatus {
  Draft = 0,
  Confirmed = 1,
  PartiallyReceived = 2,
  FullyReceived = 3,
  Cancelled = 4,
}

export const purchaseOrderStatusOptions = mapEnumToOptions(PurchaseOrderStatus);
