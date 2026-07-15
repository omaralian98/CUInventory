import {
  Component,
  ContentChildren,
  EventEmitter,
  Input,
  Output,
  QueryList,
  TemplateRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ListService, LocalizationPipe } from '@abp/ng.core';
import { Subject } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { ColumnDirective } from './column.directive';
import { ColumnConfig, RowAction } from './data-table.types';

/**
 * Config-driven list built on a Bootstrap table + ABP ListService. Owns paging,
 * sorting and the search box; the parent folds `searchChange` into its query and
 * supplies rows/count from the hooked query. Reused by every list page.
 */
@Component({
  selector: 'cu-data-table',
  standalone: true,
  imports: [CommonModule, FormsModule, LocalizationPipe],
  templateUrl: './data-table.component.html',
  styleUrls: ['./data-table.component.scss'],
})
export class DataTableComponent {
  @Input({ required: true }) columns: ColumnConfig[] = [];
  @Input() rows: any[] | null = [];
  @Input() count: number | null = 0;
  @Input({ required: true }) list!: ListService;
  @Input() actions: RowAction[] = [];
  @Input() rowKey = 'id';

  @Input() showSearch = false;
  @Input() showToolbar = false;
  @Input() searchPlaceholder = '::Search';
  @Input() emptyText = '::NoRecords';
  @Input() loading = false;

  @Output() searchChange = new EventEmitter<string>();
  @Output() action = new EventEmitter<{ key: string; row: any }>();

  @ContentChildren(ColumnDirective) cellTemplates!: QueryList<ColumnDirective>;

  readonly pageSizes = [10, 25, 50];
  private searchInput$ = new Subject<string>();

  constructor() {
    this.searchInput$.pipe(debounceTime(300)).subscribe(term => {
      this.list.page = 0;
      this.searchChange.emit(term);
    });
  }

  get pageSize(): number {
    return this.list?.maxResultCount ?? 10;
  }
  get currentPage(): number {
    return this.list?.page ?? 0;
  }
  get totalCount(): number {
    return this.count ?? 0;
  }
  get firstRow(): number {
    return this.totalCount === 0 ? 0 : this.currentPage * this.pageSize + 1;
  }
  get lastRow(): number {
    return Math.min((this.currentPage + 1) * this.pageSize, this.totalCount);
  }
  get lastPage(): number {
    return Math.max(0, Math.ceil(this.totalCount / this.pageSize) - 1);
  }

  onSearchInput(term: string): void {
    this.searchInput$.next(term);
  }

  toggleSort(col: ColumnConfig): void {
    if (!col.sortable) return;
    const isSame = this.list.sortKey === col.prop;
    const nextOrder = isSame && this.list.sortOrder === 'asc' ? 'desc' : 'asc';
    this.list.sortKey = col.prop;
    this.list.sortOrder = nextOrder;
    this.list.page = 0;
    this.list.get();
  }

  sortIcon(col: ColumnConfig): string {
    if (!col.sortable || this.list.sortKey !== col.prop) return 'fa-sort';
    return this.list.sortOrder === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
  }

  changePageSize(size: number): void {
    this.list.maxResultCount = Number(size);
    this.list.page = 0;
    this.list.get();
  }

  goTo(page: number): void {
    this.list.page = Math.max(0, Math.min(page, this.lastPage));
    this.list.get();
  }

  templateFor(name: string | undefined): TemplateRef<any> | null {
    if (!name) return null;
    return this.cellTemplates?.find(t => t.prop === name)?.template ?? null;
  }

  valueOf(row: any, prop: string): any {
    return prop.split('.').reduce((acc, key) => (acc == null ? acc : acc[key]), row);
  }

  trackRow = (_: number, row: any) => row?.[this.rowKey] ?? row;

  visibleActions(row: any): RowAction[] {
    return this.actions.filter(a => !a.visible || a.visible(row));
  }
}
