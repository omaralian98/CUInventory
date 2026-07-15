import { Injectable, computed, inject, signal } from '@angular/core';
import { ToasterService } from '@abp/ng.theme.shared';
import { LookupService } from '../lookup/lookup.service';
import { StockNotificationClient } from './stock-notification.client';
import { StockNotificationDto } from './stock-notification.model';

const FEED_CAP = 50;

/**
 * Single app-wide subscription to the stock SSE stream. Holds the latest balance per
 * inventory balance and a rolling low-stock feed as signals; raises a toast whenever a
 * product crosses below its threshold. Dashboard, the balances page and the global
 * toast all read from here — one connection, many consumers.
 */
@Injectable({ providedIn: 'root' })
export class StockNotificationStore {
  private client = inject(StockNotificationClient);
  private toaster = inject(ToasterService);
  private lookup = inject(LookupService);

  readonly connected = signal(false);
  readonly balances = signal<Map<string, StockNotificationDto>>(new Map());
  readonly feed = signal<StockNotificationDto[]>([]);

  readonly lowStockItems = computed(() =>
    [...this.balances().values()].filter(b => b.isBelowThreshold),
  );
  readonly lowStockCount = computed(() => this.lowStockItems().length);

  private disconnect: (() => void) | null = null;
  private reconnectDelay = 3000;
  private reconnectTimer: ReturnType<typeof setTimeout> | null = null;
  private started = false;

  start(): void {
    if (this.started) return;
    this.started = true;
    this.lookup.load();
    this.open();
  }

  reconnect(): void {
    if (!this.started) {
      this.start();
      return;
    }
    if (this.reconnectTimer) {
      clearTimeout(this.reconnectTimer);
      this.reconnectTimer = null;
    }
    this.disconnect?.();
    this.reconnectDelay = 3000;
    this.open();
  }

  stop(): void {
    this.started = false;
    if (this.reconnectTimer) clearTimeout(this.reconnectTimer);
    this.disconnect?.();
    this.disconnect = null;
    this.connected.set(false);
  }

  private open(): void {
    this.disconnect = this.client.connect({
      onConnected: () => {
        this.connected.set(true);
        this.reconnectDelay = 3000;
      },
      onEvent: (name, payload) => this.ingest(name, payload),
      onError: () => {
        this.connected.set(false);
        this.scheduleReconnect();
      },
    });
  }

  private scheduleReconnect(): void {
    if (!this.started || this.reconnectTimer) return;
    this.reconnectTimer = setTimeout(() => {
      this.reconnectTimer = null;
      this.reconnectDelay = Math.min(this.reconnectDelay * 2, 30000);
      this.open();
    }, this.reconnectDelay);
  }

  private ingest(name: string, payload: StockNotificationDto): void {
    // Update the per-balance snapshot.
    const next = new Map(this.balances());
    next.set(payload.inventoryBalanceId, payload);
    this.balances.set(next);

    if (name === 'LowStockReached' || payload.isBelowThreshold) {
      this.feed.set([payload, ...this.feed()].slice(0, FEED_CAP));
      const params = [
        this.lookup.nameOf('product', payload.productId),
        this.lookup.nameOf('warehouse', payload.warehouseId),
        String(payload.quantityAvailable),
      ];
      const withThreshold = payload.lowStockThreshold != null;
      this.toaster.warn(
        withThreshold ? '::LowStock:ToastWithThreshold' : '::LowStock:Toast',
        '::LowStock:ToastTitle',
        {
          life: 6000,
          messageLocalizationParams: withThreshold
            ? [...params, String(payload.lowStockThreshold)]
            : params,
        },
      );
    }
  }
}
