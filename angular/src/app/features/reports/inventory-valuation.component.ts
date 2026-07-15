import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LocalizationPipe, LocalizationService } from '@abp/ng.core';
import { ReportsService } from '../../proxy/reporting/reports.service';
import { InventoryValuationItemDto, InventoryValuationReportDto } from '../../proxy/reporting/dtos/models';
import {
  PageShellComponent,
  StatTileComponent,
  DonutChartComponent,
  ReportFilterBarComponent,
  PagerComponent,
  IdNamePipe,
  LookupService,
} from '../../shared';
import { DonutDatum } from '../../shared/charts/donut-chart.component';
import { ReportFilterFields } from '../../shared/report-filter-bar/report-filter-bar.component';

@Component({
  selector: 'cu-inventory-valuation',
  standalone: true,
  imports: [CommonModule, LocalizationPipe, PageShellComponent, StatTileComponent, DonutChartComponent, ReportFilterBarComponent, PagerComponent, IdNamePipe],
  templateUrl: './inventory-valuation.component.html',
})
export class InventoryValuationComponent {
  private service = inject(ReportsService);
  private lookup = inject(LookupService);
  private localization = inject(LocalizationService);

  loading = signal(false);
  report = signal<InventoryValuationReportDto | null>(null);
  page = signal(0);
  readonly pageSize = 20;
  private filter: ReportFilterFields = {};
  private chartItems = signal<InventoryValuationItemDto[]>([]);

  items = computed<InventoryValuationItemDto[]>(() => this.report()?.items ?? []);

  valueByCategory = computed<DonutDatum[]>(() => {
    const map = new Map<string, number>();
    for (const it of this.chartItems()) {
      const key = it.categoryId ?? 'uncategorized';
      map.set(key, (map.get(key) ?? 0) + (it.totalValue ?? 0));
    }
    return [...map.entries()].map(([id, value]) => ({
      label:
        id === 'uncategorized'
          ? this.localization.instant('::Uncategorized')
          : this.lookup.nameOf('category', id),
      value,
    }));
  });

  constructor() {
    this.lookup.load();
  }

  onFilter(f: ReportFilterFields): void {
    this.filter = f;
    this.page.set(0);
    this.load();
    this.loadChart();
  }

  goTo(page: number): void {
    this.page.set(page);
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.service.getInventoryValuation({ ...this.filter, skipCount: this.page() * this.pageSize, maxResultCount: this.pageSize }).subscribe({
      next: r => {
        this.report.set(r);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  private loadChart(): void {
    this.service.getInventoryValuation({ ...this.filter, maxResultCount: 1000, skipCount: 0 }).subscribe(r => {
      this.chartItems.set(r.items ?? []);
    });
  }
}
