import { describe, expect, it } from 'vitest';
import { enumEntry, enumLabel, enumOptions } from './enum-labels';

describe('enum-labels', () => {
  it('maps a known numeric value to its localization key and tone', () => {
    expect(enumLabel('sale-status', 1)).toBe('::Confirmed');
    expect(enumEntry('sale-status', 1).tone).toBe('success');
    expect(enumLabel('transfer-status', 2)).toBe('::Received');
    expect(enumLabel('po-status', 2)).toBe('::PartiallyReceived');
  });

  it('falls back to a neutral dash for null/undefined', () => {
    expect(enumLabel('sale-status', null)).toBe('—');
    expect(enumLabel('sale-status', undefined)).toBe('—');
    expect(enumEntry('sale-status', null).tone).toBe('neutral');
  });

  it('returns the raw value for an unknown key', () => {
    expect(enumLabel('sale-status', 99)).toBe('99');
  });

  it('produces select options preserving numeric values', () => {
    const opts = enumOptions('allocation-kind');
    expect(opts).toContainEqual({ value: 0, label: '::Fifo' });
    expect(opts.every(o => typeof o.value === 'number')).toBe(true);
    expect(opts).toHaveLength(4);
  });
});
