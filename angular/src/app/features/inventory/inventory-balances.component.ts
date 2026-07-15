import { Component, effect, inject, signal, untracked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ListService } from '@abp/ng.core';
import { finalize } from 'rxjs/operators';
import { InventoryBalanceService } from '../../proxy/inventory/inventory-balance.service';
import { InventoryBalanceDto } from '../../proxy/inventory/dtos/models';
import {
  PageShellComponent,
  DataTableComponent,
  ColumnDirective,
  ModalComponent,
  AutocompleteComponent,
  IdNamePipe,
  AuditInfoComponent,
  ColumnConfig,
  RowAction,
  LookupService,
} from '../../shared';
import { StockNotificationStore } from '../../shared/realtime/stock-notification.store';
import { ListPageBase } from '../shared/list-page.base';

@Component({
  selector: 'cu-inventory-balances',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PageShellComponent,
    DataTableComponent,
    ColumnDirective,
    ModalComponent,
    AutocompleteComponent,
    IdNamePipe,
    AuditInfoComponent,
  ],
  providers: [ListService],
  templateUrl: './inventory-balances.component.html',
})
export class InventoryBalancesComponent extends ListPageBase<InventoryBalanceDto> {
  private service = inject(InventoryBalanceService);
  lookup = inject(LookupService);
  store = inject(StockNotificationStore);

  warehouseId: string | null = null;
  productId: string | null = null;
  lowStockOnly = false;

  thresholdModalOpen = signal(false);
  savingThreshold = signal(false);
  thresholdTarget = signal<InventoryBalanceDto | null>(null);
  thresholdValue: number | null = null;
  detailOpen = signal(false);
  detailRow = signal<InventoryBalanceDto | null>(null);

  columns: ColumnConfig[] = [
    { prop: 'warehouseId', header: 'Warehouse', cell: 'warehouse' },
    { prop: 'productId', header: 'Product', cell: 'product' },
    { prop: 'quantityOnHand', header: 'On hand', pipe: 'number', align: 'end', sortable: true },
    { prop: 'quantityReserved', header: 'Reserved', pipe: 'number', align: 'end' },
    { prop: 'quantityAvailable', header: 'Available', cell: 'available', align: 'end' },
    { prop: 'lowStockThreshold', header: 'Threshold', pipe: 'number', align: 'end' },
  ];

  actions: RowAction[] = [
    { key: 'details', label: 'Details', icon: 'fa-circle-info' },
    { key: 'threshold', label: 'Set threshold', icon: 'fa-bell' },
  ];

  // Live: when the SSE store reports a new snapshot for a balance we're showing,
  // patch that row's quantities in place so managers see changes without refreshing.
  private liveEffect = effect(() => {
    const snapshots = this.store.balances();
    if (snapshots.size === 0) return;
    untracked(() => {
      let changed = false;
      const patched = this.rows().map(row => {
        const snap = row.id ? snapshots.get(row.id) : undefined;
        if (!snap) return row;
        changed = true;
        return {
          ...row,
          quantityOnHand: snap.quantityOnHand,
          quantityReserved: snap.quantityReserved,
          quantityAvailable: snap.quantityAvailable,
          lowStockThreshold: snap.lowStockThreshold ?? row.lowStockThreshold,
        };
      });
      if (changed) this.rows.set(patched);
    });
  });

  constructor() {
    super();
    this.lookup.load();
    this.store.start();
    this.hook(query =>
      this.service.getList({
        ...query,
        warehouseId: this.warehouseId,
        productId: this.productId,
        lowStockOnly: this.lowStockOnly,
      }),
    );
  }

  onFilter(): void {
    this.list.page = 0;
    this.reload();
  }

  clearFilters(): void {
    this.warehouseId = null;
    this.productId = null;
    this.lowStockOnly = false;
    this.onFilter();
  }

  isLow(row: InventoryBalanceDto): boolean {
    return row.lowStockThreshold != null && (row.quantityAvailable ?? 0) <= row.lowStockThreshold;
  }

  onAction(e: { key: string; row: InventoryBalanceDto }): void {
    if (e.key === 'threshold') this.openThreshold(e.row);
    else if (e.key === 'details') {
      this.detailRow.set(e.row);
      this.detailOpen.set(true);
    }
  }

  private openThreshold(row: InventoryBalanceDto): void {
    this.thresholdTarget.set(row);
    this.thresholdValue = row.lowStockThreshold ?? null;
    this.thresholdModalOpen.set(true);
  }

  saveThreshold(): void {
    const target = this.thresholdTarget();
    if (!target) return;
    this.savingThreshold.set(true);
    this.service
      .setLowStockThreshold(target.id!, {
        threshold: this.thresholdValue ?? null,
        concurrencyStamp: target.concurrencyStamp,
      })
      .pipe(finalize(() => this.savingThreshold.set(false)))
      .subscribe({
        next: () => {
          this.toaster.success('Threshold updated.');
          this.thresholdModalOpen.set(false);
          this.reload();
        },
        error: err => this.toaster.error(err?.error?.error?.message ?? 'Update failed.', 'Error'),
      });
  }
}
