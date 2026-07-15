import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportsService } from '../../proxy/reporting/reports.service';
import { SalesBySourceItemDto, SalesBySourceReportDto, SalesSourceDetailDto } from '../../proxy/reporting/dtos/models';
import {
  PageShellComponent,
  StatTileComponent,
  BarChartComponent,
  ReportFilterBarComponent,
  PagerComponent,
  IdNamePipe,
  LookupService,
} from '../../shared';
import { BarDatum } from '../../shared/charts/bar-chart.component';
import { ReportFilterFields } from '../../shared/report-filter-bar/report-filter-bar.component';

@Component({
  selector: 'cu-sales-by-source',
  standalone: true,
  imports: [CommonModule, PageShellComponent, StatTileComponent, BarChartComponent, ReportFilterBarComponent, PagerComponent, IdNamePipe],
  templateUrl: './sales-by-source.component.html',
})
export class SalesBySourceComponent {
  private service = inject(ReportsService);
  private lookup = inject(LookupService);

  loading = signal(false);
  report = signal<SalesBySourceReportDto | null>(null);
  page = signal(0);
  detail = signal<SalesSourceDetailDto[]>([]);
  detailCount = signal(0);
  detailPage = signal(0);
  showDetail = signal(false);
  readonly pageSize = 20;
  private filter: ReportFilterFields = {};
  private chartItems = signal<SalesBySourceItemDto[]>([]);

  items = computed<SalesBySourceItemDto[]>(() => this.report()?.items ?? []);

  // Gross margin by product — the "which source is profitable" view.
  marginByProduct = computed<BarDatum[]>(() => {
    const map = new Map<string, number>();
    for (const it of this.chartItems()) {
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
    this.page.set(0);
    this.detailPage.set(0);
    this.load();
    this.loadChart();
  }

  goTo(page: number): void {
    this.page.set(page);
    this.load();
  }

  goToDetail(page: number): void {
    this.detailPage.set(page);
    this.loadDetail();
  }

  toggleDetail(): void {
    this.showDetail.update(v => !v);
    if (this.showDetail() && this.detail().length === 0) this.loadDetail();
  }

  private load(): void {
    this.loading.set(true);
    this.service.getSalesBySource({ ...this.filter, skipCount: this.page() * this.pageSize, maxResultCount: this.pageSize }).subscribe({
      next: r => {
        this.report.set(r);
        this.loading.set(false);
        if (this.showDetail()) this.loadDetail();
      },
      error: () => this.loading.set(false),
    });
  }

  private loadChart(): void {
    this.service.getSalesBySource({ ...this.filter, maxResultCount: 1000, skipCount: 0 }).subscribe(r => {
      this.chartItems.set(r.items ?? []);
    });
  }

  private loadDetail(): void {
    this.service
      .getSalesBySourceDetail({ ...this.filter, skipCount: this.detailPage() * this.pageSize, maxResultCount: this.pageSize })
      .subscribe(res => {
        this.detail.set(res.items ?? []);
        this.detailCount.set(res.totalCount ?? 0);
      });
  }
}
