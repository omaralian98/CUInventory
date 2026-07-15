// Central place mapping every backend enum (serialized as an integer) to a human
// label and a visual tone. Consumed by cu-status-badge and the various selects.

export type BadgeTone = 'success' | 'warning' | 'danger' | 'info' | 'neutral';

export interface EnumEntry {
  label: string;
  tone: BadgeTone;
}

export type EnumKind =
  | 'sale-status'
  | 'transfer-status'
  | 'shipment-status'
  | 'po-status'
  | 'lot-source'
  | 'allocation-kind'
  | 'active';

const MAPS: Record<EnumKind, Record<number, EnumEntry>> = {
  active: {
    0: { label: 'Inactive', tone: 'neutral' },
    1: { label: 'Active', tone: 'success' },
  },
  'sale-status': {
    0: { label: 'Draft', tone: 'neutral' },
    1: { label: 'Confirmed', tone: 'success' },
    2: { label: 'Cancelled', tone: 'danger' },
  },
  'transfer-status': {
    0: { label: 'Draft', tone: 'neutral' },
    1: { label: 'Dispatched', tone: 'info' },
    2: { label: 'Received', tone: 'success' },
    3: { label: 'Cancelled', tone: 'danger' },
  },
  'shipment-status': {
    0: { label: 'Draft', tone: 'neutral' },
    1: { label: 'Dispatched', tone: 'info' },
    2: { label: 'Received', tone: 'success' },
  },
  'po-status': {
    0: { label: 'Draft', tone: 'neutral' },
    1: { label: 'Confirmed', tone: 'info' },
    2: { label: 'Partially Received', tone: 'warning' },
    3: { label: 'Fully Received', tone: 'success' },
    4: { label: 'Cancelled', tone: 'danger' },
  },
  'lot-source': {
    0: { label: 'Purchase', tone: 'info' },
    1: { label: 'Transfer In', tone: 'neutral' },
  },
  'allocation-kind': {
    0: { label: 'FIFO', tone: 'neutral' },
    1: { label: 'Specific Lot', tone: 'info' },
    2: { label: 'Specific Supplier', tone: 'info' },
    3: { label: 'Specific Warehouse', tone: 'info' },
  },
};

export function enumEntry(kind: EnumKind, value: number | undefined | null): EnumEntry {
  if (value == null) return { label: '—', tone: 'neutral' };
  return MAPS[kind][value] ?? { label: String(value), tone: 'neutral' };
}

export function enumLabel(kind: EnumKind, value: number | undefined | null): string {
  return enumEntry(kind, value).label;
}

export interface SelectOption {
  value: number;
  label: string;
}

export function enumOptions(kind: EnumKind): SelectOption[] {
  return Object.entries(MAPS[kind]).map(([value, entry]) => ({
    value: Number(value),
    label: entry.label,
  }));
}
