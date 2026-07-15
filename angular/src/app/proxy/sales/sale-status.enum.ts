import { mapEnumToOptions } from '@abp/ng.core';

export enum SaleStatus {
  Draft = 0,
  Confirmed = 1,
  Cancelled = 2,
}

export const saleStatusOptions = mapEnumToOptions(SaleStatus);
