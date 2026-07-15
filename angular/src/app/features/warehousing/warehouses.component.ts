import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ListService, PermissionDirective } from '@abp/ng.core';
import { finalize } from 'rxjs/operators';
import { WarehouseService } from '../../proxy/warehousing/warehouse.service';
import { WarehouseDto } from '../../proxy/warehousing/dtos/models';
import {
  PageShellComponent,
  DataTableComponent,
  ColumnDirective,
  FormFieldComponent,
  ModalComponent,
  StatusBadgeComponent,
  ColumnConfig,
  RowAction,
  LookupService,
} from '../../shared';
import { ListPageBase } from '../shared/list-page.base';

@Component({
  selector: 'cu-warehouses',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    PermissionDirective,
    PageShellComponent,
    DataTableComponent,
    ColumnDirective,
    FormFieldComponent,
    ModalComponent,
    StatusBadgeComponent,
  ],
  providers: [ListService],
  templateUrl: './warehouses.component.html',
})
export class WarehousesComponent extends ListPageBase<WarehouseDto> {
  private service = inject(WarehouseService);
  private fb = inject(FormBuilder);
  private lookup = inject(LookupService);

  filterText = '';
  includeInactive = false;

  modalOpen = signal(false);
  saving = signal(false);
  editing = signal<WarehouseDto | null>(null);

  form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(256)]],
    code: ['', [Validators.required, Validators.maxLength(64)]],
    address: this.fb.group({
      governorate: ['', [Validators.required, Validators.maxLength(128)]],
      city: ['', [Validators.required, Validators.maxLength(128)]],
      street: ['', [Validators.required, Validators.maxLength(256)]],
    }),
    orderIndex: [0],
    isActive: [true],
  });

  get addressGroup(): FormGroup {
    return this.form.get('address') as FormGroup;
  }

  columns: ColumnConfig[] = [
    { prop: 'name', header: 'Name', sortable: true },
    { prop: 'code', header: 'Code', sortable: true },
    { prop: 'address.city', header: 'City' },
    { prop: 'isActive', header: 'Status', cell: 'status' },
    { prop: 'creationTime', header: 'Created', pipe: 'date', sortable: true },
  ];

  actions: RowAction[] = [
    { key: 'edit', label: 'Edit', icon: 'fa-pen' },
    { key: 'delete', label: 'Delete', icon: 'fa-trash-can', tone: 'danger' },
  ];

  constructor() {
    super();
    this.hook(query =>
      this.service.getList({ ...query, filter: this.filterText, includeInactive: this.includeInactive }),
    );
  }

  onSearch(term: string): void {
    this.filterText = term;
    this.reload();
  }

  onToggleInactive(): void {
    this.includeInactive = !this.includeInactive;
    this.list.page = 0;
    this.reload();
  }

  create(): void {
    this.editing.set(null);
    this.form.reset({
      name: '',
      code: '',
      address: { governorate: '', city: '', street: '' },
      orderIndex: 0,
      isActive: true,
    });
    this.modalOpen.set(true);
  }

  onAction(e: { key: string; row: WarehouseDto }): void {
    if (e.key === 'edit') this.edit(e.row);
    else if (e.key === 'delete') this.remove(e.row);
  }

  private edit(row: WarehouseDto): void {
    this.service.get(row.id!).subscribe(dto => {
      this.editing.set(dto);
      this.form.reset({
        name: dto.name ?? '',
        code: dto.code ?? '',
        address: {
          governorate: dto.address?.governorate ?? '',
          city: dto.address?.city ?? '',
          street: dto.address?.street ?? '',
        },
        orderIndex: dto.orderIndex ?? 0,
        isActive: dto.isActive ?? true,
      });
      this.modalOpen.set(true);
    });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving.set(true);
    const v = this.form.getRawValue();
    const address = {
      governorate: v.address.governorate!,
      city: v.address.city!,
      street: v.address.street!,
    };
    const current = this.editing();
    const request = current
      ? this.service.update(current.id!, {
          name: v.name!,
          code: v.code!,
          address,
          orderIndex: v.orderIndex ?? 0,
          isActive: v.isActive ?? true,
          concurrencyStamp: current.concurrencyStamp,
        })
      : this.service.create({
          name: v.name!,
          code: v.code!,
          address,
          orderIndex: v.orderIndex ?? 0,
          isActive: v.isActive ?? true,
        });

    request.pipe(finalize(() => this.saving.set(false))).subscribe({
      next: () => {
        this.toaster.success(current ? 'Warehouse updated.' : 'Warehouse created.');
        this.modalOpen.set(false);
        this.lookup.refresh();
        this.reload();
      },
      error: err => this.toaster.error(err?.error?.error?.message ?? 'Save failed.', 'Error'),
    });
  }

  private remove(row: WarehouseDto): void {
    this.confirmAction(
      `Delete warehouse "${row.name}"?`,
      'Delete warehouse',
      () => this.service.delete(row.id!),
      'Warehouse deleted.',
    );
  }
}
