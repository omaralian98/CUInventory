import { mapEnumToOptions } from '@abp/ng.core';

export enum StockTransferStatus {
  Draft = 0,
  Dispatched = 1,
  Received = 2,
  Cancelled = 3,
}

export const stockTransferStatusOptions = mapEnumToOptions(StockTransferStatus);
