import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ListService, LocalizationPipe, PermissionDirective } from '@abp/ng.core';
import { finalize } from 'rxjs/operators';
import { ProductService } from '../../proxy/catalog/product.service';
import { ProductDto } from '../../proxy/catalog/dtos/models';
import {
  PageShellComponent,
  DataTableComponent,
  ColumnDirective,
  FormFieldComponent,
  ModalComponent,
  StatusBadgeComponent,
  AutocompleteComponent,
  IdNamePipe,
  AuditInfoComponent,
  ColumnConfig,
  RowAction,
  LookupService,
} from '../../shared';
import { ListPageBase } from '../shared/list-page.base';

@Component({
  selector: 'cu-products',
  standalone: true,
  imports: [
    CommonModule,
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
    AuditInfoComponent,
  ],
  providers: [ListService],
  templateUrl: './products.component.html',
})
export class ProductsComponent extends ListPageBase<ProductDto> {
  private service = inject(ProductService);
  private fb = inject(FormBuilder);
  lookup = inject(LookupService);

  filterText = '';
  includeInactive = false;

  modalOpen = signal(false);
  saving = signal(false);
  editing = signal<ProductDto | null>(null);
  detailOpen = signal(false);
  detailRow = signal<ProductDto | null>(null);

  form = this.fb.group({
    name: ['', Validators.required],
    sku: [''],
    description: [''],
    categoryId: [null as string | null],
    isService: [false],
    isActive: [true],
    orderIndex: [0],
  });

  columns: ColumnConfig[] = [
    { prop: 'name', header: '::Name', sortable: true },
    { prop: 'sku', header: '::Sku', sortable: true },
    { prop: 'categoryId', header: '::Category', cell: 'category' },
    { prop: 'isService', header: '::Type', cell: 'type' },
    { prop: 'isActive', header: '::Status', cell: 'status' },
  ];

  actions: RowAction[] = [
    { key: 'details', label: '::Details', icon: 'fa-circle-info' },
    { key: 'edit', label: '::Edit', icon: 'fa-pen' },
    { key: 'delete', label: '::Delete', icon: 'fa-trash-can', tone: 'danger' },
  ];

  constructor() {
    super();
    this.lookup.load();
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
    this.form.reset({ name: '', sku: '', description: '', categoryId: null, isService: false, isActive: true, orderIndex: 0 });
    this.modalOpen.set(true);
  }

  onAction(e: { key: string; row: ProductDto }): void {
    if (e.key === 'details') this.showDetails(e.row);
    else if (e.key === 'edit') this.edit(e.row);
    else if (e.key === 'delete') this.remove(e.row);
  }

  private showDetails(row: ProductDto): void {
    this.detailRow.set(row);
    this.detailOpen.set(true);
  }

  private edit(row: ProductDto): void {
    this.service.get(row.id!).subscribe(dto => {
      this.editing.set(dto);
      this.form.reset({
        name: dto.name ?? '',
        sku: dto.sku ?? '',
        description: dto.description ?? '',
        categoryId: dto.categoryId ?? null,
        isService: dto.isService ?? false,
        isActive: dto.isActive ?? true,
        orderIndex: dto.orderIndex ?? 0,
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
    const current = this.editing();
    const request = current
      ? this.service.update(current.id!, { ...v, name: v.name!, concurrencyStamp: current.concurrencyStamp })
      : this.service.create({ ...v, name: v.name! });

    request.pipe(finalize(() => this.saving.set(false))).subscribe({
      next: () => {
        this.toaster.success(current ? '::Products:Updated' : '::Products:Created');
        this.modalOpen.set(false);
        this.lookup.refresh();
        this.reload();
      },
      error: err => this.toaster.error(err?.error?.error?.message ?? '::SaveFailed', '::Error'),
    });
  }

  private remove(row: ProductDto): void {
    this.confirmAction(
      '::Products:ConfirmDelete',
      '::Products:ConfirmDeleteTitle',
      () => this.service.delete(row.id!),
      '::Products:Deleted',
      [row.name ?? ''],
    );
  }
}
