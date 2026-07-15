import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportsService } from '../../proxy/reporting/reports.service';
import { InventoryValuationItemDto, InventoryValuationReportDto } from '../../proxy/reporting/dtos/models';
import {
  PageShellComponent,
  StatTileComponent,
  DonutChartComponent,
  ReportFilterBarComponent,
  IdNamePipe,
  LookupService,
} from '../../shared';
import { DonutDatum } from '../../shared/charts/donut-chart.component';
import { ReportFilterFields } from '../../shared/report-filter-bar/report-filter-bar.component';

@Component({
  selector: 'cu-inventory-valuation',
  standalone: true,
  imports: [CommonModule, PageShellComponent, StatTileComponent, DonutChartComponent, ReportFilterBarComponent, IdNamePipe],
  templateUrl: './inventory-valuation.component.html',
})
export class InventoryValuationComponent {
  private service = inject(ReportsService);
  private lookup = inject(LookupService);

  loading = signal(false);
  report = signal<InventoryValuationReportDto | null>(null);
  private filter: ReportFilterFields = {};

  items = computed<InventoryValuationItemDto[]>(() => this.report()?.items ?? []);

  valueByCategory = computed<DonutDatum[]>(() => {
    const map = new Map<string, number>();
    for (const it of this.items()) {
      const key = it.categoryId ?? 'uncategorized';
      map.set(key, (map.get(key) ?? 0) + (it.totalValue ?? 0));
    }
    return [...map.entries()].map(([id, value]) => ({
      label: id === 'uncategorized' ? 'Uncategorized' : this.lookup.nameOf('category', id),
      value,
    }));
  });

  constructor() {
    this.lookup.load();
  }

  onFilter(f: ReportFilterFields): void {
    this.filter = f;
    this.loading.set(true);
    this.service.getInventoryValuation({ ...this.filter, maxResultCount: 1000, skipCount: 0 }).subscribe({
      next: r => {
        this.report.set(r);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
