import { mapEnumToOptions } from '@abp/ng.core';

export enum InventoryLotSource {
  Purchase = 0,
  TransferIn = 1,
}

export const inventoryLotSourceOptions = mapEnumToOptions(InventoryLotSource);
