import { mapEnumToOptions } from '@abp/ng.core';

export enum ShipmentStatus {
  Draft = 0,
  Dispatched = 1,
  Received = 2,
}

export const shipmentStatusOptions = mapEnumToOptions(ShipmentStatus);
