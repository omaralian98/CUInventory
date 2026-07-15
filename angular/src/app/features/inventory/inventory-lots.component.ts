import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ListService, LocalizationPipe } from '@abp/ng.core';
import { InventoryLotService } from '../../proxy/inventory/inventory-lot.service';
import { InventoryLotDto } from '../../proxy/inventory/dtos/models';
import {
  PageShellComponent,
  DataTableComponent,
  ColumnDirective,
  StatusBadgeComponent,
  AutocompleteComponent,
  ModalComponent,
  IdNamePipe,
  AuditInfoComponent,
  ColumnConfig,
  RowAction,
  LookupService,
} from '../../shared';
import { ListPageBase } from '../shared/list-page.base';

@Component({
  selector: 'cu-inventory-lots',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    LocalizationPipe,
    PageShellComponent,
    DataTableComponent,
    ColumnDirective,
    StatusBadgeComponent,
    AutocompleteComponent,
    ModalComponent,
    IdNamePipe,
    AuditInfoComponent,
  ],
  providers: [ListService],
  templateUrl: './inventory-lots.component.html',
})
export class InventoryLotsComponent extends ListPageBase<InventoryLotDto> {
  private service = inject(InventoryLotService);
  lookup = inject(LookupService);

  warehouseId: string | null = null;
  productId: string | null = null;
  supplierId: string | null = null;
  hasRemaining = false;

  detailOpen = signal(false);
  detailRow = signal<InventoryLotDto | null>(null);

  actions: RowAction[] = [{ key: 'details', label: '::Details', icon: 'fa-circle-info' }];

  columns: ColumnConfig[] = [
    { prop: 'productId', header: '::Product', cell: 'product' },
    { prop: 'warehouseId', header: '::Warehouse', cell: 'warehouse' },
    { prop: 'supplierId', header: '::Supplier', cell: 'supplier' },
    { prop: 'source', header: '::Source', cell: 'source' },
    { prop: 'originalQuantity', header: '::Original', pipe: 'number', align: 'end' },
    { prop: 'remainingQuantity', header: '::Remaining', pipe: 'number', align: 'end', sortable: true },
    { prop: 'unitCost', header: '::UnitCost', pipe: 'money', align: 'end' },
    { prop: 'receivedAt', header: '::Received', pipe: 'date', sortable: true },
  ];

  constructor() {
    super();
    this.lookup.load();
    this.hook(query =>
      this.service.getList({
        ...query,
        warehouseId: this.warehouseId,
        productId: this.productId,
        supplierId: this.supplierId,
        hasRemaining: this.hasRemaining,
      }),
    );
  }

  onAction(e: { key: string; row: InventoryLotDto }): void {
    if (e.key === 'details') {
      this.detailRow.set(e.row);
      this.detailOpen.set(true);
    }
  }

  onFilter(): void {
    this.list.page = 0;
    this.reload();
  }

  clearFilters(): void {
    this.warehouseId = null;
    this.productId = null;
    this.supplierId = null;
    this.hasRemaining = false;
    this.onFilter();
  }
}
