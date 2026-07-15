import { describe, expect, it, vi, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { ToasterService } from '@abp/ng.theme.shared';
import { LookupService } from '../lookup/lookup.service';
import { StockNotificationClient, StockStreamHandlers } from './stock-notification.client';
import { StockNotificationStore } from './stock-notification.store';
import { StockNotificationDto, StockNotificationType } from './stock-notification.model';

function notification(overrides: Partial<StockNotificationDto>): StockNotificationDto {
  return {
    type: StockNotificationType.StockChanged,
    inventoryBalanceId: 'bal-1',
    warehouseId: 'wh-1',
    productId: 'prod-1',
    quantityOnHand: 10,
    quantityReserved: 0,
    quantityAvailable: 10,
    lowStockThreshold: 5,
    isBelowThreshold: false,
    occurredAt: new Date().toISOString(),
    tenantId: null,
    ...overrides,
  };
}

describe('StockNotificationStore', () => {
  let store: StockNotificationStore;
  let handlers: StockStreamHandlers;
  let warn: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    handlers = undefined as unknown as StockStreamHandlers;
    warn = vi.fn();

    TestBed.configureTestingModule({
      providers: [
        StockNotificationStore,
        { provide: ToasterService, useValue: { warn } },
        { provide: LookupService, useValue: { load: () => {}, nameOf: () => 'Widget' } },
        {
          provide: StockNotificationClient,
          useValue: {
            connect: (h: StockStreamHandlers) => {
              handlers = h;
              h.onConnected?.();
              return () => {};
            },
          },
        },
      ],
    });

    store = TestBed.inject(StockNotificationStore);
    store.start();
  });

  it('marks the stream connected once opened', () => {
    expect(store.connected()).toBe(true);
  });

  it('tracks the latest balance snapshot per inventory balance', () => {
    handlers.onEvent('StockChanged', notification({ quantityAvailable: 8 }));
    handlers.onEvent('StockChanged', notification({ quantityAvailable: 6 }));

    expect(store.balances().size).toBe(1);
    expect(store.balances().get('bal-1')?.quantityAvailable).toBe(6);
    expect(store.lowStockCount()).toBe(0);
  });

  it('pushes low-stock events onto the feed, raises a toast and counts them', () => {
    handlers.onEvent('LowStockReached', notification({ quantityAvailable: 2, isBelowThreshold: true }));

    expect(store.feed()).toHaveLength(1);
    expect(store.lowStockCount()).toBe(1);
    expect(warn).toHaveBeenCalledOnce();
  });

  it('clears the low-stock flag when stock recovers above threshold', () => {
    handlers.onEvent('LowStockReached', notification({ quantityAvailable: 2, isBelowThreshold: true }));
    expect(store.lowStockCount()).toBe(1);

    handlers.onEvent('StockChanged', notification({ quantityAvailable: 12, isBelowThreshold: false }));
    expect(store.lowStockCount()).toBe(0);
  });
});
