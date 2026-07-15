import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportsService } from '../../proxy/reporting/reports.service';
import { LowStockItemDto } from '../../proxy/reporting/dtos/models';
import { PageShellComponent, StatTileComponent, ReportFilterBarComponent, PagerComponent, IdNamePipe, LookupService } from '../../shared';
import { ReportFilterFields } from '../../shared/report-filter-bar/report-filter-bar.component';

@Component({
  selector: 'cu-low-stock',
  standalone: true,
  imports: [CommonModule, PageShellComponent, StatTileComponent, ReportFilterBarComponent, PagerComponent, IdNamePipe],
  templateUrl: './low-stock.component.html',
})
export class LowStockComponent {
  private service = inject(ReportsService);
  private lookup = inject(LookupService);

  loading = signal(false);
  rows = signal<LowStockItemDto[]>([]);
  count = signal(0);
  page = signal(0);
  readonly pageSize = 20;
  private filter: ReportFilterFields = {};

  lastPage = computed(() => Math.max(0, Math.ceil(this.count() / this.pageSize) - 1));

  constructor() {
    this.lookup.load();
  }

  onFilter(f: ReportFilterFields): void {
    this.filter = f;
    this.page.set(0);
    this.load();
  }

  goTo(page: number): void {
    this.page.set(Math.max(0, Math.min(page, this.lastPage())));
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.service
      .getLowStock({ ...this.filter, skipCount: this.page() * this.pageSize, maxResultCount: this.pageSize })
      .subscribe({
        next: res => {
          this.rows.set(res.items ?? []);
          this.count.set(res.totalCount ?? 0);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }
}
