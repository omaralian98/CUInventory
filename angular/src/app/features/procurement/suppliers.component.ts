import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ListService, LocalizationPipe, PermissionDirective } from '@abp/ng.core';
import { finalize } from 'rxjs/operators';
import { SupplierService } from '../../proxy/procurement/supplier.service';
import { SupplierDto } from '../../proxy/procurement/dtos/models';
import {
  PageShellComponent,
  DataTableComponent,
  FormFieldComponent,
  ModalComponent,
  AuditInfoComponent,
  ColumnConfig,
  RowAction,
  LookupService,
  phoneValidator,
} from '../../shared';
import { ListPageBase } from '../shared/list-page.base';

@Component({
  selector: 'cu-suppliers',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    PermissionDirective,
    LocalizationPipe,
    PageShellComponent,
    DataTableComponent,
    FormFieldComponent,
    ModalComponent,
    AuditInfoComponent,
  ],
  providers: [ListService],
  templateUrl: './suppliers.component.html',
})
export class SuppliersComponent extends ListPageBase<SupplierDto> {
  private service = inject(SupplierService);
  private fb = inject(FormBuilder);
  private lookup = inject(LookupService);

  filterText = '';

  modalOpen = signal(false);
  saving = signal(false);
  editing = signal<SupplierDto | null>(null);
  detailOpen = signal(false);
  detailRow = signal<SupplierDto | null>(null);

  form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(256)]],
    contact: this.fb.group({
      email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
      phoneNumber: ['', [Validators.required, phoneValidator, Validators.maxLength(32)]],
      address: this.fb.group({
        governorate: ['', [Validators.required, Validators.maxLength(128)]],
        city: ['', [Validators.required, Validators.maxLength(128)]],
        street: ['', [Validators.required, Validators.maxLength(256)]],
      }),
    }),
  });

  get contactGroup(): FormGroup {
    return this.form.get('contact') as FormGroup;
  }

  get contactAddressGroup(): FormGroup {
    return this.contactGroup.get('address') as FormGroup;
  }

  columns: ColumnConfig[] = [
    { prop: 'name', header: '::Name', sortable: true },
    { prop: 'contact.email', header: '::Email' },
    { prop: 'contact.phoneNumber', header: '::Phone' },
    { prop: 'contact.address.city', header: '::City' },
  ];

  actions: RowAction[] = [
    { key: 'details', label: '::Details', icon: 'fa-circle-info' },
    { key: 'edit', label: '::Edit', icon: 'fa-pen' },
    { key: 'delete', label: '::Delete', icon: 'fa-trash-can', tone: 'danger' },
  ];

  constructor() {
    super();
    this.hook(query => this.service.getList({ ...query, filter: this.filterText }));
  }

  onSearch(term: string): void {
    this.filterText = term;
    this.reload();
  }

  create(): void {
    this.editing.set(null);
    this.form.reset({
      name: '',
      contact: {
        email: '',
        phoneNumber: '',
        address: { governorate: '', city: '', street: '' },
      },
    });
    this.modalOpen.set(true);
  }

  onAction(e: { key: string; row: SupplierDto }): void {
    if (e.key === 'details') this.showDetails(e.row);
    else if (e.key === 'edit') this.edit(e.row);
    else if (e.key === 'delete') this.remove(e.row);
  }

  private showDetails(row: SupplierDto): void {
    this.detailRow.set(row);
    this.detailOpen.set(true);
  }

  private edit(row: SupplierDto): void {
    this.service.get(row.id!).subscribe(dto => {
      this.editing.set(dto);
      this.form.reset({
        name: dto.name ?? '',
        contact: {
          email: dto.contact?.email ?? '',
          phoneNumber: dto.contact?.phoneNumber ?? '',
          address: {
            governorate: dto.contact?.address?.governorate ?? '',
            city: dto.contact?.address?.city ?? '',
            street: dto.contact?.address?.street ?? '',
          },
        },
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
    const contact = {
      email: v.contact.email!,
      phoneNumber: v.contact.phoneNumber!,
      address: {
        governorate: v.contact.address.governorate!,
        city: v.contact.address.city!,
        street: v.contact.address.street!,
      },
    };
    const current = this.editing();
    const request = current
      ? this.service.update(current.id!, { name: v.name!, contact, concurrencyStamp: current.concurrencyStamp })
      : this.service.create({ name: v.name!, contact });

    request.pipe(finalize(() => this.saving.set(false))).subscribe({
      next: () => {
        this.toaster.success(current ? '::Suppliers:Updated' : '::Suppliers:Created');
        this.modalOpen.set(false);
        this.lookup.refresh();
        this.reload();
      },
      error: err => this.toaster.error(err?.error?.error?.message ?? '::SaveFailed', '::Error'),
    });
  }

  private remove(row: SupplierDto): void {
    this.confirmAction(
      '::Suppliers:ConfirmDelete',
      '::Suppliers:ConfirmDeleteTitle',
      () => this.service.delete(row.id!),
      '::Suppliers:Deleted',
      [row.name ?? ''],
    );
  }
}
