import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportsService } from '../../proxy/reporting/reports.service';
import { SalesBySourceItemDto, SalesBySourceReportDto, SalesSourceDetailDto } from '../../proxy/reporting/dtos/models';
import {
  PageShellComponent,
  StatTileComponent,
  BarChartComponent,
  ReportFilterBarComponent,
  IdNamePipe,
  LookupService,
} from '../../shared';
import { BarDatum } from '../../shared/charts/bar-chart.component';
import { ReportFilterFields } from '../../shared/report-filter-bar/report-filter-bar.component';

@Component({
  selector: 'cu-sales-by-source',
  standalone: true,
  imports: [CommonModule, PageShellComponent, StatTileComponent, BarChartComponent, ReportFilterBarComponent, IdNamePipe],
  templateUrl: './sales-by-source.component.html',
})
export class SalesBySourceComponent {
  private service = inject(ReportsService);
  private lookup = inject(LookupService);

  loading = signal(false);
  report = signal<SalesBySourceReportDto | null>(null);
  detail = signal<SalesSourceDetailDto[]>([]);
  showDetail = signal(false);
  private filter: ReportFilterFields = {};

  items = computed<SalesBySourceItemDto[]>(() => this.report()?.items ?? []);

  // Gross margin by product — the "which source is profitable" view.
  marginByProduct = computed<BarDatum[]>(() => {
    const map = new Map<string, number>();
    for (const it of this.items()) {
      const key = it.productId ?? '—';
      map.set(key, (map.get(key) ?? 0) + (it.grossMargin ?? 0));
    }
    return [...map.entries()]
      .map(([id, value]) => ({ label: this.lookup.nameOf('product', id), value }))
      .sort((a, b) => b.value - a.value)
      .slice(0, 8);
  });

  constructor() {
    this.lookup.load();
  }

  onFilter(f: ReportFilterFields): void {
    this.filter = f;
    this.load();
  }

  toggleDetail(): void {
    this.showDetail.update(v => !v);
    if (this.showDetail() && this.detail().length === 0) this.loadDetail();
  }

  private load(): void {
    this.loading.set(true);
    this.service.getSalesBySource({ ...this.filter, maxResultCount: 1000, skipCount: 0 }).subscribe({
      next: r => {
        this.report.set(r);
        this.loading.set(false);
        if (this.showDetail()) this.loadDetail();
      },
      error: () => this.loading.set(false),
    });
  }

  private loadDetail(): void {
    this.service.getSalesBySourceDetail({ ...this.filter, maxResultCount: 200, skipCount: 0 }).subscribe(res => {
      this.detail.set(res.items ?? []);
    });
  }
}
