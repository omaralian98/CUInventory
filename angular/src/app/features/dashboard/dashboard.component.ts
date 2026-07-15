import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LocalizationPipe, LocalizationService } from '@abp/ng.core';
import { forkJoin } from 'rxjs';
import { ReportsService } from '../../proxy/reporting/reports.service';
import { StockTransferService } from '../../proxy/inventory/stock-transfer.service';
import { SaleService } from '../../proxy/sales/sale.service';
import { StockTransferStatus } from '../../proxy/inventory/stock-transfer-status.enum';
import { SaleStatus } from '../../proxy/sales/sale-status.enum';
import {
  PageShellComponent,
  StatTileComponent,
  DonutChartComponent,
  IdNamePipe,
  LookupService,
  StockNotificationStore,
} from '../../shared';
import { DonutDatum } from '../../shared/charts/donut-chart.component';

@Component({
  selector: 'cu-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, LocalizationPipe, PageShellComponent, StatTileComponent, DonutChartComponent, IdNamePipe],
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent {
  private reports = inject(ReportsService);
  private transfers = inject(StockTransferService);
  private sales = inject(SaleService);
  private lookup = inject(LookupService);
  private localization = inject(LocalizationService);
  store = inject(StockNotificationStore);

  loading = signal(true);
  totalValue = signal(0);
  lowStockBaseline = signal(0);
  pendingTransfers = signal(0);
  draftSales = signal(0);
  valueByCategory = signal<DonutDatum[]>([]);

  // Live low-stock count combines the report baseline with anything the SSE feed has flagged.
  liveLowStock = computed(() => Math.max(this.lowStockBaseline(), this.store.lowStockCount()));
  feed = computed(() => this.store.feed().slice(0, 8));

  constructor() {
    this.lookup.load();
    this.store.start();
    this.loadKpis();
  }

  private loadKpis(): void {
    forkJoin({
      valuation: this.reports.getInventoryValuation({ maxResultCount: 1000, skipCount: 0 }),
      lowStock: this.reports.getLowStock({ maxResultCount: 1, skipCount: 0 }),
      transfers: this.transfers.getList({ status: StockTransferStatus.Dispatched, maxResultCount: 1, skipCount: 0 }),
      draftSales: this.sales.getList({ status: SaleStatus.Draft, maxResultCount: 1, skipCount: 0 }),
    }).subscribe({
      next: ({ valuation, lowStock, transfers, draftSales }) => {
        this.totalValue.set(valuation.grandTotalValue ?? 0);
        this.lowStockBaseline.set(lowStock.totalCount ?? 0);
        this.pendingTransfers.set(transfers.totalCount ?? 0);
        this.draftSales.set(draftSales.totalCount ?? 0);

        const map = new Map<string, number>();
        for (const it of valuation.items ?? []) {
          const key = it.categoryId ?? 'uncategorized';
          map.set(key, (map.get(key) ?? 0) + (it.totalValue ?? 0));
        }
        this.valueByCategory.set(
          [...map.entries()].map(([id, value]) => ({
            label:
              id === 'uncategorized'
                ? this.localization.instant('::Uncategorized')
                : this.lookup.nameOf('category', id),
            value,
          })),
        );
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
