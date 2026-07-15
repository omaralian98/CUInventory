import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ListService, PermissionDirective } from '@abp/ng.core';
import { of } from 'rxjs';
import { finalize, map } from 'rxjs/operators';
import { ShipmentService } from '../../proxy/warehousing/shipment.service';
import { ShipmentDto } from '../../proxy/warehousing/dtos/models';
import { PurchaseOrderService } from '../../proxy/procurement/purchase-order.service';
import { PurchaseOrderDto, PurchaseOrderLineDto } from '../../proxy/procurement/dtos/models';
import { PurchaseOrderStatus } from '../../proxy/procurement/purchase-order-status.enum';
import {
  PageShellComponent,
  DataTableComponent,
  ColumnDirective,
  FormFieldComponent,
  ModalComponent,
  StatusBadgeComponent,
  AutocompleteComponent,
  AutocompleteOption,
  IdNamePipe,
  LineItemsComponent,
  ColumnConfig,
  RowAction,
  LookupService,
  enumOptions,
  takenProductIds,
} from '../../shared';
import { LineRowDirective } from '../../shared/line-items/line-items.component';
import { ListPageBase } from '../shared/list-page.base';

@Component({
  selector: 'cu-shipments',
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
  templateUrl: './shipments.component.html',
})
export class ShipmentsComponent extends ListPageBase<ShipmentDto> {
  private service = inject(ShipmentService);
  private purchaseOrders = inject(PurchaseOrderService);
  private fb = inject(FormBuilder);
  lookup = inject(LookupService);

  readonly statusOptions = enumOptions('shipment-status');

  warehouseFilter: string | null = null;
  statusFilter: number | null = null;

  modalOpen = signal(false);
  saving = signal(false);
  detailOpen = signal(false);
  detail = signal<ShipmentDto | null>(null);
  poLines = signal<PurchaseOrderLineDto[]>([]);

  newLine = (): FormGroup =>
    this.fb.group({
      productId: [null as string | null, Validators.required],
      quantity: [1, [Validators.required, Validators.min(0.0001)]],
      unitCost: [0, [Validators.required, Validators.min(0)]],
    });

  form = this.fb.group({
    purchaseOrderId: [null as string | null, Validators.required],
    supplierId: [null as string | null, Validators.required],
    destinationWarehouseId: [null as string | null, Validators.required],
    lines: this.fb.array([this.newLine()]),
  });

  columns: ColumnConfig[] = [
    { prop: 'creationTime', header: 'Created', pipe: 'date', sortable: true },
    { prop: 'supplierId', header: 'Supplier', cell: 'supplier' },
    { prop: 'destinationWarehouseId', header: 'Warehouse', cell: 'warehouse' },
    { prop: 'status', header: 'Status', cell: 'status' },
    { prop: 'dispatchedAt', header: 'Dispatched', pipe: 'date' },
    { prop: 'receivedAt', header: 'Received', pipe: 'date' },
  ];

  actions: RowAction[] = [
    { key: 'view', label: 'View', icon: 'fa-eye' },
    { key: 'dispatch', label: 'Dispatch', icon: 'fa-truck-fast', visible: r => r.status === 0 },
    { key: 'receive', label: 'Receive', icon: 'fa-box-open', visible: r => r.status === 1 },
    { key: 'delete', label: 'Delete', icon: 'fa-trash-can', tone: 'danger', visible: r => r.status === 0 },
  ];

  constructor() {
    super();
    this.lookup.load();
    this.hook(query =>
      this.service.getList({
        ...query,
        destinationWarehouseId: this.warehouseFilter,
        status: this.statusFilter,
      }),
    );
    this.form.get('purchaseOrderId')!.valueChanges.subscribe(id => {
      if (!id) {
        this.poLines.set([]);
        this.resetLines();
      }
    });
  }

  get lines(): FormArray {
    return this.form.get('lines') as FormArray;
  }

  applyFilters(): void {
    this.list.page = 0;
    this.reload();
  }

  clearFilters(): void {
    this.warehouseFilter = null;
    this.statusFilter = null;
    this.applyFilters();
  }

  poLabel(po: PurchaseOrderDto): string {
    const supplier = this.lookup.nameOf('supplier', po.supplierId);
    const when = po.creationTime ? new Date(po.creationTime).toLocaleDateString() : '';
    return `${supplier} — ${when}`;
  }

  searchPurchaseOrders = (term: string) =>
    this.purchaseOrders
      .getList({
        filter: term || undefined,
        statuses: [PurchaseOrderStatus.Confirmed, PurchaseOrderStatus.PartiallyReceived],
        maxResultCount: 20,
        skipCount: 0,
      })
      .pipe(map(res => (res.items ?? []).map(po => ({ id: po.id!, name: this.poLabel(po), data: po }))));

  create(): void {
    this.form.reset({ purchaseOrderId: null, supplierId: null, destinationWarehouseId: null });
    this.poLines.set([]);
    this.resetLines();
    this.modalOpen.set(true);
  }

  onPoSelected(option: AutocompleteOption): void {
    const po = option.data as PurchaseOrderDto;
    this.form.patchValue({ supplierId: po.supplierId, destinationWarehouseId: po.destinationWarehouseId });
    this.resetLines();
    this.poLines.set([]);
    this.purchaseOrders.get(po.id!).subscribe(full => this.poLines.set(full.lines ?? []));
  }

  searchPoProducts = (group: FormGroup) => (term: string) => {
    const taken = takenProductIds(this.lines, group);
    const needle = term.trim().toLowerCase();
    const options = this.poLines()
      .filter(l => !!l.productId && !l.isFullyReceived && !taken.has(l.productId!))
      .map(l => ({ id: l.productId!, name: this.lookup.nameOf('product', l.productId), data: l }))
      .filter(o => !needle || o.name.toLowerCase().includes(needle));
    return of(options);
  };

  onLineProductSelected(group: FormGroup, option: AutocompleteOption): void {
    const line = option.data as PurchaseOrderLineDto | undefined;
    if (!line) return;
    group.patchValue({ quantity: line.outstandingQuantity, unitCost: line.unitCost });
    group.get('quantity')?.setValidators([
      Validators.required,
      Validators.min(0.0001),
      Validators.max(line.outstandingQuantity ?? 0),
    ]);
    group.get('quantity')?.updateValueAndValidity();
  }

  private resetLines(): void {
    this.lines.clear();
    this.lines.push(this.newLine());
  }

  onAction(e: { key: string; row: ShipmentDto }): void {
    switch (e.key) {
      case 'view':
        this.openDetail(e.row);
        break;
      case 'dispatch':
        this.runAction(
          () => this.service.dispatch(e.row.id!, { concurrencyStamp: e.row.concurrencyStamp }),
          'Shipment dispatched.',
        );
        break;
      case 'receive':
        this.runAction(
          () => this.service.receive(e.row.id!, { concurrencyStamp: e.row.concurrencyStamp }),
          'Shipment received — inventory lots created and stock updated.',
        );
        break;
      case 'delete':
        this.confirmAction(
          'Delete this draft shipment?',
          'Delete shipment',
          () => this.service.delete(e.row.id!),
          'Shipment deleted.',
        );
        break;
    }
  }

  private openDetail(row: ShipmentDto): void {
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
        purchaseOrderId: v.purchaseOrderId!,
        supplierId: v.supplierId!,
        destinationWarehouseId: v.destinationWarehouseId!,
        lines: (v.lines as any[]).map(l => ({ productId: l.productId, quantity: l.quantity, unitCost: l.unitCost })),
      })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.toaster.success('Shipment created.');
          this.modalOpen.set(false);
          this.reload();
        },
        error: err => this.toaster.error(err?.error?.error?.message ?? 'Save failed.', 'Error'),
      });
  }
}
