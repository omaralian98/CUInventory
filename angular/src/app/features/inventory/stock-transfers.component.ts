import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ListService, LocalizationPipe, PermissionDirective } from '@abp/ng.core';
import { finalize } from 'rxjs/operators';
import { StockTransferService } from '../../proxy/inventory/stock-transfer.service';
import { StockTransferDto } from '../../proxy/inventory/dtos/models';
import { StockTransferStatus } from '../../proxy/inventory/stock-transfer-status.enum';
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
  AuditInfoComponent,
  ColumnConfig,
  RowAction,
  LookupService,
  enumOptions,
  productLineSearch,
} from '../../shared';
import { LineRowDirective } from '../../shared/line-items/line-items.component';
import { ListPageBase } from '../shared/list-page.base';

@Component({
  selector: 'cu-stock-transfers',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    PermissionDirective,
    LocalizationPipe,
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
    AuditInfoComponent,
  ],
  providers: [ListService],
  templateUrl: './stock-transfers.component.html',
})
export class StockTransfersComponent extends ListPageBase<StockTransferDto> {
  private service = inject(StockTransferService);
  private products = inject(ProductService);
  private fb = inject(FormBuilder);
  lookup = inject(LookupService);

  productSearch = productLineSearch(this.products, () => this.lines);

  readonly Status = StockTransferStatus;
  statusOptions = enumOptions('transfer-status');

  filter = { sourceWarehouseId: null as string | null, destinationWarehouseId: null as string | null, status: null as StockTransferStatus | null };

  createOpen = signal(false);
  detailOpen = signal(false);
  saving = signal(false);
  detail = signal<StockTransferDto | null>(null);

  newLine = (): FormGroup =>
    this.fb.group({
      productId: [null as string | null, Validators.required],
      quantity: [1, [Validators.required, Validators.min(0.0001)]],
    });

  form = this.fb.group({
    sourceWarehouseId: [null as string | null, Validators.required],
    destinationWarehouseId: [null as string | null, Validators.required],
    lines: this.fb.array([this.newLine()]),
  });

  columns: ColumnConfig[] = [
    { prop: 'creationTime', header: '::Created', pipe: 'date', sortable: true },
    { prop: 'sourceWarehouseId', header: '::From', cell: 'from' },
    { prop: 'destinationWarehouseId', header: '::To', cell: 'to' },
    { prop: 'linesCount', header: '::Lines', cell: 'lines', align: 'end' },
    { prop: 'status', header: '::Status', cell: 'status' },
  ];

  actions: RowAction[] = [
    { key: 'view', label: '::View', icon: 'fa-eye' },
    { key: 'dispatch', label: '::Dispatch', icon: 'fa-truck-fast', visible: r => r.status === StockTransferStatus.Draft },
    { key: 'receive', label: '::Receive', icon: 'fa-box-open', visible: r => r.status === StockTransferStatus.Dispatched },
    { key: 'cancel', label: '::Cancel', icon: 'fa-ban', tone: 'danger', visible: r => r.status === StockTransferStatus.Draft || r.status === StockTransferStatus.Dispatched },
    { key: 'delete', label: '::Delete', icon: 'fa-trash-can', tone: 'danger', visible: r => r.status === StockTransferStatus.Draft },
  ];

  constructor() {
    super();
    this.lookup.load();
    this.hook(query =>
      this.service.getList({
        ...query,
        sourceWarehouseId: this.filter.sourceWarehouseId,
        destinationWarehouseId: this.filter.destinationWarehouseId,
        status: this.filter.status,
      }),
    );
  }

  get lines(): FormArray {
    return this.form.get('lines') as FormArray;
  }

  applyFilter(): void {
    this.list.page = 0;
    this.reload();
  }

  clearFilters(): void {
    this.filter = { sourceWarehouseId: null, destinationWarehouseId: null, status: null };
    this.applyFilter();
  }

  create(): void {
    this.form.reset({ sourceWarehouseId: null, destinationWarehouseId: null });
    this.lines.clear();
    this.lines.push(this.newLine());
    this.createOpen.set(true);
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    if (v.sourceWarehouseId === v.destinationWarehouseId) {
      this.toaster.error('::StockTransfers:SameWarehouseError', '::StockTransfers:InvalidTransfer');
      return;
    }
    this.saving.set(true);
    this.service
      .create({
        sourceWarehouseId: v.sourceWarehouseId!,
        destinationWarehouseId: v.destinationWarehouseId!,
        lines: (v.lines ?? []).map(l => ({ productId: l.productId!, quantity: l.quantity! })),
      })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.toaster.success('::StockTransfers:Created');
          this.createOpen.set(false);
          this.reload();
        },
        error: err => this.toaster.error(err?.error?.error?.message ?? '::SaveFailed', '::Error'),
      });
  }

  onAction(e: { key: string; row: StockTransferDto }): void {
    const row = e.row;
    const stamp = { concurrencyStamp: row.concurrencyStamp };
    switch (e.key) {
      case 'view':
        this.openDetail(row);
        break;
      case 'dispatch':
        this.runAction(() => this.service.dispatch(row.id!, stamp), '::StockTransfers:Dispatched');
        break;
      case 'receive':
        this.runAction(() => this.service.receive(row.id!, stamp), '::StockTransfers:Received');
        break;
      case 'cancel':
        this.confirmAction('::StockTransfers:ConfirmCancel', '::StockTransfers:ConfirmCancelTitle', () => this.service.cancel(row.id!, stamp), '::StockTransfers:Cancelled');
        break;
      case 'delete':
        this.confirmAction('::StockTransfers:ConfirmDelete', '::StockTransfers:ConfirmDeleteTitle', () => this.service.delete(row.id!), '::StockTransfers:Deleted');
        break;
    }
  }

  private openDetail(row: StockTransferDto): void {
    this.detail.set(row);
    this.detailOpen.set(true);
    // Refresh from server so allocations reflect the latest dispatch state.
    this.service.get(row.id!).subscribe(full => this.detail.set(full));
  }

  timelineStep(status: StockTransferStatus | undefined): number {
    switch (status) {
      case StockTransferStatus.Draft: return 0;
      case StockTransferStatus.Dispatched: return 1;
      case StockTransferStatus.Received: return 2;
      case StockTransferStatus.Cancelled: return -1;
      default: return 0;
    }
  }
}
