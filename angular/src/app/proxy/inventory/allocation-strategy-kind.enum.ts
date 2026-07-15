import { mapEnumToOptions } from '@abp/ng.core';

export enum AllocationStrategyKind {
  Fifo = 0,
  SpecificLot = 1,
  SpecificSupplier = 2,
  SpecificWarehouse = 3,
}

export const allocationStrategyKindOptions = mapEnumToOptions(AllocationStrategyKind);
