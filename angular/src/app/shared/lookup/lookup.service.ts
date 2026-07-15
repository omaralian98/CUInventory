import { Injectable, computed, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { ProductService } from '../../proxy/catalog/product.service';
import { CategoryService } from '../../proxy/catalog/category.service';
import { SupplierService } from '../../proxy/procurement/supplier.service';
import { WarehouseService } from '../../proxy/warehousing/warehouse.service';

export type LookupKind = 'product' | 'category' | 'supplier' | 'warehouse';

export interface LookupItem {
  id: string;
  name: string;
}

/**
 * Loads and caches the reference lists (products, categories, suppliers, warehouses)
 * as signals, so every table/report can resolve a Guid to a name and every form can
 * offer a picker without re-fetching. Reports return only IDs — this is what makes them readable.
 */
@Injectable({ providedIn: 'root' })
export class LookupService {
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);
  private supplierService = inject(SupplierService);
  private warehouseService = inject(WarehouseService);

  readonly products = signal<LookupItem[]>([]);
  readonly categories = signal<LookupItem[]>([]);
  readonly suppliers = signal<LookupItem[]>([]);
  readonly warehouses = signal<LookupItem[]>([]);
  readonly loaded = signal(false);

  private maps: Record<LookupKind, Map<string, string>> = {
    product: new Map(),
    category: new Map(),
    supplier: new Map(),
    warehouse: new Map(),
  };

  private started = false;

  /** Idempotent: loads all reference lists once. Safe to call from every consumer. */
  load(): void {
    if (this.started) return;
    this.started = true;
    const page = { maxResultCount: 1000, skipCount: 0 };
    forkJoin({
      products: this.productService.getList({ ...page, includeInactive: true }),
      categories: this.categoryService.getList({ ...page, includeInactive: true }),
      suppliers: this.supplierService.getList({ ...page }),
      warehouses: this.warehouseService.getList({ ...page, includeInactive: true }),
    }).subscribe(({ products, categories, suppliers, warehouses }) => {
      this.set('product', this.products, (products.items ?? []).map(p => ({ id: p.id!, name: p.name ?? '' })));
      this.set('category', this.categories, (categories.items ?? []).map(c => ({ id: c.id!, name: c.name ?? '' })));
      this.set('supplier', this.suppliers, (suppliers.items ?? []).map(s => ({ id: s.id!, name: s.name ?? '' })));
      this.set('warehouse', this.warehouses, (warehouses.items ?? []).map(w => ({ id: w.id!, name: w.name ?? '' })));
      this.loaded.set(true);
    });
  }

  /** Force a refresh (e.g. after creating a product) so pickers stay current. */
  refresh(): void {
    this.started = false;
    this.load();
  }

  nameOf(kind: LookupKind, id: string | null | undefined): string {
    if (!id) return '—';
    return this.maps[kind].get(id) ?? '—';
  }

  optionsOf(kind: LookupKind): LookupItem[] {
    switch (kind) {
      case 'product': return this.products();
      case 'category': return this.categories();
      case 'supplier': return this.suppliers();
      case 'warehouse': return this.warehouses();
    }
  }

  readonly warehouseOptions = computed(() => this.warehouses());
  readonly supplierOptions = computed(() => this.suppliers());
  readonly categoryOptions = computed(() => this.categories());
  readonly productOptions = computed(() => this.products());

  private set(kind: LookupKind, sig: ReturnType<typeof signal<LookupItem[]>>, items: LookupItem[]): void {
    const map = new Map<string, string>();
    for (const item of items) map.set(item.id, item.name);
    this.maps[kind] = map;
    sig.set(items);
  }
}
