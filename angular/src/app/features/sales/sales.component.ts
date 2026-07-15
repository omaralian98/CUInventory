import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormArray, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ListService, LocalizationPipe, LocalizationService, PermissionDirective } from '@abp/ng.core';
import { of } from 'rxjs';
import { finalize, map } from 'rxjs/operators';
import { SaleService } from '../../proxy/sales/sale.service';
import { InventoryLotService } from '../../proxy/inventory/inventory-lot.service';
import { SaleDto } from '../../proxy/sales/dtos/models';
import { SaleStatus } from '../../proxy/sales/sale-status.enum';
import { AllocationStrategyKind } from '../../proxy/inventory/allocation-strategy-kind.enum';
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
  selector: 'cu-sales',
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
  templateUrl: './sales.component.html',
})
export class SalesComponent extends ListPageBase<SaleDto> {
  private service = inject(SaleService);
  private products = inject(ProductService);
  private lots = inject(InventoryLotService);
  private fb = inject(FormBuilder);
  private localization = inject(LocalizationService);
  private destroyRef = inject(DestroyRef);
  lookup = inject(LookupService);

  productSearch = productLineSearch(this.products, () => this.lines);

  readonly Status = SaleStatus;
  readonly Kind = AllocationStrategyKind;
  statusOptions = enumOptions('sale-status');
  kindOptions = enumOptions('allocation-kind');

  filterStatus: SaleStatus | null = null;

  createOpen = signal(false);
  detailOpen = signal(false);
  saving = signal(false);
  detail = signal<SaleDto | null>(null);

  /** Each strategy pins its source to exactly one of these controls; the rest stay empty. */
  private static readonly pinnedControls = [
    { name: 'warehouseId', kind: AllocationStrategyKind.SpecificWarehouse },
    { name: 'supplierId', kind: AllocationStrategyKind.SpecificSupplier },
    { name: 'lotId', kind: AllocationStrategyKind.SpecificLot },
  ];

  newLine = (): FormGroup => {
    const group = this.fb.group({
      productId: [null as string | null, Validators.required],
      quantity: [1, [Validators.required, Validators.min(0.0001)]],
      unitPrice: [0, [Validators.required, Validators.min(0)]],
      kind: [AllocationStrategyKind.Fifo, Validators.required],
      warehouseId: [null as string | null],
      supplierId: [null as string | null],
      lotId: [null as string | null],
    });

    this.applyStrategyValidators(group, AllocationStrategyKind.Fifo);

    group
      .get('kind')!
      .valueChanges.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(kind => this.applyStrategyValidators(group, kind));

    group
      .get('productId')!
      .valueChanges.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.clear(group.get('lotId')!));

    return group;
  };

  private applyStrategyValidators(group: FormGroup, kind: AllocationStrategyKind | null): void {
    for (const pinned of SalesComponent.pinnedControls) {
      const control = group.get(pinned.name)!;
      if (pinned.kind === kind) {
        control.setValidators(Validators.required);
        control.updateValueAndValidity({ emitEvent: false });
      } else {
        control.clearValidators();
        this.clear(control);
      }
    }
  }

  private clear(control: AbstractControl): void {
    control.setValue(null, { emitEvent: false });
    control.updateValueAndValidity({ emitEvent: false });
  }

  form = this.fb.group({
    lines: this.fb.array([this.newLine()]),
  });

  columns: ColumnConfig[] = [
    { prop: 'creationTime', header: '::Created', pipe: 'date', sortable: true },
    { prop: 'linesCount', header: '::Items', pipe: 'number', align: 'end' },
    { prop: 'totalAmount', header: '::Total', pipe: 'money', align: 'end' },
    { prop: 'status', header: '::Status', cell: 'status' },
    { prop: 'confirmedAt', header: '::Confirmed', pipe: 'datetime' },
  ];

  actions: RowAction[] = [
    { key: 'view', label: '::View', icon: 'fa-eye' },
    { key: 'confirm', label: '::Confirm', icon: 'fa-check', visible: r => r.status === SaleStatus.Draft },
    { key: 'cancel', label: '::Cancel', icon: 'fa-ban', tone: 'danger', visible: r => r.status === SaleStatus.Draft },
    { key: 'delete', label: '::Delete', icon: 'fa-trash-can', tone: 'danger', visible: r => r.status === SaleStatus.Draft },
  ];

  constructor() {
    super();
    this.lookup.load();
    this.hook(query => this.service.getList({ ...query, status: this.filterStatus }));
  }

  get lines(): FormArray {
    return this.form.get('lines') as FormArray;
  }

  applyFilter(): void {
    this.list.page = 0;
    this.reload();
  }

  create(): void {
    this.lines.clear();
    this.lines.push(this.newLine());
    this.createOpen.set(true);
  }

  lineTotal(line: { quantity?: number; unitPrice?: number }): number {
    return (line.quantity ?? 0) * (line.unitPrice ?? 0);
  }

  lotSearch = (group: FormGroup) => (term: string) => {
    const productId = group.get('productId')?.value as string | null;
    if (!productId) return of([]);
    const needle = term.trim().toLowerCase();
    return this.lots
      .getList({ productId, availableOnly: true, maxResultCount: 20, skipCount: 0 })
      .pipe(
        map(res =>
          (res.items ?? [])
            .map(l => ({
              id: l.id!,
              name: `${l.id!.slice(0, 8)} · ${this.localization.instant('::Sales:Avail')} ${l.availableQuantity ?? 0} · ${l.receivedAt ? new Date(l.receivedAt).toLocaleDateString() : ''}`,
            }))
            .filter(o => !needle || o.name.toLowerCase().includes(needle)),
        ),
      );
  };

  formTotal(): number {
    return (this.lines.getRawValue() as any[]).reduce((s, l) => s + this.lineTotal(l), 0);
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving.set(true);
    const lines = (this.lines.getRawValue() as any[]).map(l => ({
      productId: l.productId,
      quantity: l.quantity,
      unitPrice: l.unitPrice,
      kind: l.kind,
      warehouseId: l.kind === AllocationStrategyKind.SpecificWarehouse ? l.warehouseId : null,
      supplierId: l.kind === AllocationStrategyKind.SpecificSupplier ? l.supplierId : null,
      lotId: l.kind === AllocationStrategyKind.SpecificLot ? l.lotId : null,
    }));

    this.service
      .create({ lines })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.toaster.success('::Sales:Created');
          this.createOpen.set(false);
          this.reload();
        },
        error: err => this.toaster.error(err?.error?.error?.message ?? '::Sales:CreateFailed', '::Sales:CreateFailedTitle'),
      });
  }

  onAction(e: { key: string; row: SaleDto }): void {
    const row = e.row;
    const stamp = { concurrencyStamp: row.concurrencyStamp };
    switch (e.key) {
      case 'view':
        this.openDetail(row);
        break;
      case 'confirm':
        this.runAction(() => this.service.confirm(row.id!, stamp), '::Sales:Confirmed');
        break;
      case 'cancel':
        this.confirmAction('::Sales:ConfirmCancel', '::Sales:ConfirmCancelTitle', () => this.service.cancel(row.id!, stamp), '::Sales:Cancelled');
        break;
      case 'delete':
        this.confirmAction('::Sales:ConfirmDelete', '::Sales:ConfirmDeleteTitle', () => this.service.delete(row.id!), '::Sales:Deleted');
        break;
    }
  }

  private openDetail(row: SaleDto): void {
    this.detail.set(row);
    this.detailOpen.set(true);
    this.service.get(row.id!).subscribe(full => this.detail.set(full));
  }
}
