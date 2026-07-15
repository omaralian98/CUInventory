import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ListService, PermissionDirective } from '@abp/ng.core';
import { finalize } from 'rxjs/operators';
import { PurchaseOrderService } from '../../proxy/procurement/purchase-order.service';
import { PurchaseOrderDto } from '../../proxy/procurement/dtos/models';
import { ProductService } from '../../proxy/catalog/product.service';
import {
  PageShellComponent,
  DataTableComponent,
  ColumnDirective,
  FormFieldComponent,
  ModalComponent,
  StatusBadgeComponent,
  AutocompleteComponent,
  IdNamePipe,
  LineItemsComponent,
  ColumnConfig,
  RowAction,
  LookupService,
  enumOptions,
  productLineSearch,
} from '../../shared';
import { LineRowDirective } from '../../shared/line-items/line-items.component';
import { ListPageBase } from '../shared/list-page.base';

@Component({
  selector: 'cu-purchase-orders',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    PermissionDirective,
    PageShellComponent,
    DataTableComponent,
    ColumnDirective,
    FormFieldComponent,
    ModalComponent,
    StatusBadgeComponent,
    AutocompleteComponent,
    IdNamePipe,
    LineItemsComponent,
    LineRowDirective,
  ],
  providers: [ListService],
  templateUrl: './purchase-orders.component.html',
})
export class PurchaseOrdersComponent extends ListPageBase<PurchaseOrderDto> {
  private service = inject(PurchaseOrderService);
  private products = inject(ProductService);
  private fb = inject(FormBuilder);
  lookup = inject(LookupService);

  productSearch = productLineSearch(this.products, () => this.lines);

  readonly statusOptions = enumOptions('po-status');

  supplierFilter: string | null = null;
  warehouseFilter: string | null = null;
  statusFilter: number | null = null;

  modalOpen = signal(false);
  saving = signal(false);
  detailOpen = signal(false);
  detail = signal<PurchaseOrderDto | null>(null);

  newLine = (): FormGroup =>
    this.fb.group({
      productId: [null as string | null, Validators.required],
      orderedQuantity: [1, [Validators.required, Validators.min(0.0001)]],
      unitCost: [0, [Validators.required, Validators.min(0)]],
    });

  form = this.fb.group({
    supplierId: [null as string | null, Validators.required],
    destinationWarehouseId: [null as string | null, Validators.required],
    lines: this.fb.array([this.newLine()]),
  });

  columns: ColumnConfig[] = [
    { prop: 'creationTime', header: 'Created', pipe: 'date', sortable: true },
    { prop: 'supplierId', header: 'Supplier', cell: 'supplier' },
    { prop: 'destinationWarehouseId', header: 'Warehouse', cell: 'warehouse' },
    { prop: 'linesCount', header: 'Lines', cell: 'lines', align: 'end' },
    { prop: 'status', header: 'Status', cell: 'status' },
  ];

  actions: RowAction[] = [
    { key: 'view', label: 'View', icon: 'fa-eye' },
    { key: 'confirm', label: 'Confirm', icon: 'fa-check', visible: r => r.status === 0 },
    { key: 'cancel', label: 'Cancel', icon: 'fa-ban', tone: 'danger', visible: r => r.status === 0 || r.status === 1 },
    { key: 'delete', label: 'Delete', icon: 'fa-trash-can', tone: 'danger', visible: r => r.status === 0 },
  ];

  constructor() {
    super();
    this.lookup.load();
    this.hook(query =>
      this.service.getList({
        ...query,
        supplierId: this.supplierFilter,
        destinationWarehouseId: this.warehouseFilter,
        status: this.statusFilter,
      }),
    );
  }

  get lines(): FormArray {
    return this.form.get('lines') as FormArray;
  }

  applyFilters(): void {
    this.list.page = 0;
    this.reload();
  }

  clearFilters(): void {
    this.supplierFilter = null;
    this.warehouseFilter = null;
    this.statusFilter = null;
    this.applyFilters();
  }

  create(): void {
    this.form.reset({ supplierId: null, destinationWarehouseId: null });
    this.lines.clear();
    this.lines.push(this.newLine());
    this.modalOpen.set(true);
  }

  onAction(e: { key: string; row: PurchaseOrderDto }): void {
    switch (e.key) {
      case 'view':
        this.openDetail(e.row);
        break;
      case 'confirm':
        this.runAction(
          () => this.service.confirm(e.row.id!, { concurrencyStamp: e.row.concurrencyStamp }),
          'Purchase order confirmed.',
        );
        break;
      case 'cancel':
        this.confirmAction(
          'Cancel this purchase order?',
          'Cancel purchase order',
          () => this.service.cancel(e.row.id!, { concurrencyStamp: e.row.concurrencyStamp }),
          'Purchase order cancelled.',
        );
        break;
      case 'delete':
        this.confirmAction(
          'Delete this draft purchase order?',
          'Delete purchase order',
          () => this.service.delete(e.row.id!),
          'Purchase order deleted.',
        );
        break;
    }
  }

  private openDetail(row: PurchaseOrderDto): void {
    this.detail.set(row);
    this.detailOpen.set(true);
    this.service.get(row.id!).subscribe(full => this.detail.set(full));
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving.set(true);
    const v = this.form.getRawValue();
    this.service
      .create({
        supplierId: v.supplierId!,
        destinationWarehouseId: v.destinationWarehouseId!,
        lines: (v.lines as any[]).map(l => ({
          productId: l.productId,
          orderedQuantity: l.orderedQuantity,
          unitCost: l.unitCost,
        })),
      })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.toaster.success('Purchase order created.');
          this.modalOpen.set(false);
          this.reload();
        },
        error: err => this.toaster.error(err?.error?.error?.message ?? 'Save failed.', 'Error'),
      });
  }
}
