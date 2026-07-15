import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LocalizationPipe } from '@abp/ng.core';
import { AutocompleteComponent } from '../autocomplete/autocomplete.component';
import { LookupService } from '../lookup/lookup.service';

export interface ReportFilterFields {
  warehouseId?: string | null;
  supplierId?: string | null;
  categoryId?: string | null;
  productId?: string | null;
  fromDate?: string | null;
  toDate?: string | null;
}

export type ReportFilterKey = 'warehouse' | 'supplier' | 'category' | 'product' | 'dates';

/** One reusable filter panel for every report. Show only the filters a report supports. */
@Component({
  selector: 'cu-report-filter-bar',
  standalone: true,
  imports: [CommonModule, FormsModule, AutocompleteComponent, LocalizationPipe],
  templateUrl: './report-filter-bar.component.html',
  styleUrls: ['./report-filter-bar.component.scss'],
})
export class ReportFilterBarComponent implements OnInit {
  lookup = inject(LookupService);

  @Input() show: ReportFilterKey[] = ['warehouse', 'supplier', 'category', 'product', 'dates'];
  @Output() apply = new EventEmitter<ReportFilterFields>();

  model: ReportFilterFields = {};

  ngOnInit(): void {
    this.lookup.load();
    this.emit();
  }

  has(key: ReportFilterKey): boolean {
    return this.show.includes(key);
  }

  emit(): void {
    this.apply.emit({ ...this.normalized() });
  }

  clear(): void {
    this.model = {};
    this.emit();
  }

  private normalized(): ReportFilterFields {
    const m = this.model;
    return {
      warehouseId: m.warehouseId || null,
      supplierId: m.supplierId || null,
      categoryId: m.categoryId || null,
      productId: m.productId || null,
      fromDate: m.fromDate || null,
      toDate: m.toDate || null,
    };
  }
}
