import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ListService, PermissionDirective } from '@abp/ng.core';
import { finalize } from 'rxjs/operators';
import { CategoryService } from '../../proxy/catalog/category.service';
import { CategoryDto } from '../../proxy/catalog/dtos/models';
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
  selector: 'cu-categories',
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
  templateUrl: './categories.component.html',
})
export class CategoriesComponent extends ListPageBase<CategoryDto> {
  private service = inject(CategoryService);
  private fb = inject(FormBuilder);
  private lookup = inject(LookupService);

  filterText = '';
  includeInactive = false;

  modalOpen = signal(false);
  saving = signal(false);
  editing = signal<CategoryDto | null>(null);

  form = this.fb.group({
    name: ['', Validators.required],
    orderIndex: [0],
    isActive: [true],
  });

  columns: ColumnConfig[] = [
    { prop: 'name', header: 'Name', sortable: true },
    { prop: 'orderIndex', header: 'Order', sortable: true, align: 'end' },
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
    this.form.reset({ name: '', orderIndex: 0, isActive: true });
    this.modalOpen.set(true);
  }

  onAction(e: { key: string; row: CategoryDto }): void {
    if (e.key === 'edit') this.edit(e.row);
    else if (e.key === 'delete') this.remove(e.row);
  }

  private edit(row: CategoryDto): void {
    this.service.get(row.id!).subscribe(dto => {
      this.editing.set(dto);
      this.form.reset({
        name: dto.name ?? '',
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
    const value = this.form.getRawValue();
    const current = this.editing();
    const request = current
      ? this.service.update(current.id!, { ...value, name: value.name!, concurrencyStamp: current.concurrencyStamp })
      : this.service.create({ ...value, name: value.name! });

    request.pipe(finalize(() => this.saving.set(false))).subscribe({
      next: () => {
        this.toaster.success(current ? 'Category updated.' : 'Category created.');
        this.modalOpen.set(false);
        this.lookup.refresh();
        this.reload();
      },
      error: err => this.toaster.error(err?.error?.error?.message ?? 'Save failed.', 'Error'),
    });
  }

  private remove(row: CategoryDto): void {
    this.confirmAction(
      `Delete category "${row.name}"?`,
      'Delete category',
      () => this.service.delete(row.id!),
      'Category deleted.',
    );
  }
}
