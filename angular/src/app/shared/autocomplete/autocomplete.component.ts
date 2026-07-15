import {
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  OnDestroy,
  OnInit,
  Output,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { LocalizationPipe } from '@abp/ng.core';
import { ControlValueAccessor, NgControl } from '@angular/forms';
import { Observable, Subject, Subscription, of } from 'rxjs';
import { catchError, debounceTime, map, switchMap, tap } from 'rxjs/operators';
import { ProductService } from '../../proxy/catalog/product.service';
import { CategoryService } from '../../proxy/catalog/category.service';
import { SupplierService } from '../../proxy/procurement/supplier.service';
import { WarehouseService } from '../../proxy/warehousing/warehouse.service';
import { LookupKind, LookupService } from '../lookup/lookup.service';

export interface AutocompleteOption {
  id: string;
  name: string;
  data?: unknown;
}

/**
 * Backend-paged typeahead picker. Every keystroke queries the server (debounced,
 * first 20 matches) instead of rendering a preloaded list. Works with both
 * formControlName and ngModel; the control value is the selected entity id.
 */
@Component({
  selector: 'cu-autocomplete',
  standalone: true,
  imports: [CommonModule, LocalizationPipe],
  template: `
    <div [class.mb-3]="!inline">
      @if (label) {
        <label class="form-label">
          {{ label | abpLocalization }} @if (required) { <span class="text-danger">*</span> }
        </label>
      }
      <div class="position-relative">
        <input
          type="text"
          class="form-control"
          [class.form-control-sm]="small"
          [class.is-invalid]="invalid"
          [placeholder]="placeholder | abpLocalization"
          [disabled]="disabled"
          [value]="display"
          autocomplete="off"
          (input)="onInput($any($event.target).value)"
          (focus)="onFocus()"
          (blur)="onBlur()"
          (keydown)="onKeydown($event)" />
        @if (value && !disabled) {
          <button
            type="button"
            class="cu-autocomplete-clear"
            tabindex="-1"
            (mousedown)="$event.preventDefault()"
            (click)="clear()">
            <i class="fas fa-xmark"></i>
          </button>
        }
        @if (open()) {
          <ul
            class="dropdown-menu show cu-autocomplete-menu"
            [style.top.px]="menuPos().top"
            [style.bottom.px]="menuPos().bottom"
            [style.left.px]="menuPos().left"
            [style.width.px]="menuPos().width"
            [style.maxHeight.px]="menuPos().maxHeight"
            (mousedown)="$event.preventDefault()">
            @if (loading()) {
              <li class="dropdown-item disabled"><i class="fas fa-circle-notch fa-spin me-2"></i>{{ '::Loading' | abpLocalization }}</li>
            } @else if (!options().length) {
              <li class="dropdown-item disabled">{{ '::NoResults' | abpLocalization }}</li>
            } @else {
              @for (opt of options(); track opt.id; let i = $index) {
                <li>
                  <button
                    type="button"
                    class="dropdown-item"
                    [class.active]="i === activeIndex()"
                    (click)="select(opt)">
                    {{ opt.name }}
                  </button>
                </li>
              }
            }
          </ul>
        }
      </div>
      @if (invalid) {
        <div class="invalid-feedback d-block">
          {{ '::Validation:Required' | abpLocalization: ((label || '::ThisField') | abpLocalization) }}
        </div>
      }
    </div>
  `,
  styles: [
    `
      /* Fixed so overflow containers (modal body, table scroll) can't clip it. */
      .cu-autocomplete-menu {
        position: fixed;
        z-index: 1080;
        overflow-y: auto;
        overflow-x: hidden;
        min-width: 0;
        padding: 0.3rem;
        border: 1px solid var(--cu-border);
        border-radius: var(--radius-md);
        background: var(--cu-surface);
        box-shadow: var(--cu-shadow-pop);
      }
      .cu-autocomplete-menu .dropdown-item {
        max-width: 100%;
        white-space: normal;
        overflow-wrap: break-word;
        padding: 0.45rem 0.65rem;
        border-radius: calc(var(--radius-md) - 0.2rem);
        font-size: 0.8rem;
        line-height: 1.35;
        color: var(--cu-text);
      }
      .cu-autocomplete-menu .dropdown-item:hover,
      .cu-autocomplete-menu .dropdown-item.active {
        background: color-mix(in srgb, var(--cu-primary) 10%, transparent);
        color: var(--cu-primary);
      }
      .cu-autocomplete-menu .dropdown-item.disabled {
        color: var(--cu-text-muted);
        background: transparent;
      }
      .cu-autocomplete-clear {
        position: absolute;
        inset-inline-end: 0.5rem;
        top: 50%;
        transform: translateY(-50%);
        border: 0;
        background: transparent;
        padding: 0 0.25rem;
        color: var(--bs-secondary-color, #6c757d);
      }
    `,
  ],
})
export class AutocompleteComponent implements ControlValueAccessor, OnInit, OnDestroy {
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);
  private supplierService = inject(SupplierService);
  private warehouseService = inject(WarehouseService);
  private lookup = inject(LookupService);
  private host = inject(ElementRef<HTMLElement>);
  ngControl = inject(NgControl, { self: true, optional: true });

  @Input() kind?: LookupKind;
  @Input() search?: (term: string) => Observable<AutocompleteOption[]>;
  @Input() label = '';
  @Input() placeholder = '::Select';
  @Input() required = false;
  @Input() inline = false;
  @Input() small = false;
  @Input() initialLabel = '';
  @Output() selected = new EventEmitter<AutocompleteOption>();

  options = signal<AutocompleteOption[]>([]);
  open = signal(false);
  loading = signal(false);
  activeIndex = signal(-1);
  menuPos = signal<{ top: number | null; bottom: number | null; left: number; width: number; maxHeight: number }>({
    top: 0,
    bottom: null,
    left: 0,
    width: 0,
    maxHeight: 280,
  });

  value: string | null = null;
  display = '';
  disabled = false;

  private selectedName = '';
  private terms = new Subject<string>();
  private sub?: Subscription;
  private onChange: (value: string | null) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  constructor() {
    if (this.ngControl) this.ngControl.valueAccessor = this;
  }

  ngOnInit(): void {
    window.addEventListener('scroll', this.reposition, true);
    window.addEventListener('resize', this.reposition);
    if (this.kind) this.lookup.load();
    this.sub = this.terms
      .pipe(
        debounceTime(200),
        tap(() => this.loading.set(true)),
        switchMap(term => this.query(term).pipe(catchError(() => of([] as AutocompleteOption[])))),
      )
      .subscribe(opts => {
        this.options.set(opts);
        this.loading.set(false);
        this.activeIndex.set(-1);
      });
  }

  ngOnDestroy(): void {
    window.removeEventListener('scroll', this.reposition, true);
    window.removeEventListener('resize', this.reposition);
    this.sub?.unsubscribe();
  }

  get invalid(): boolean {
    const c = this.ngControl?.control;
    return !!c && c.invalid && (c.dirty || c.touched);
  }

  writeValue(value: string | null): void {
    this.value = value ?? null;
    this.selectedName = this.resolveName(this.value);
    this.display = this.selectedName;
  }

  registerOnChange(fn: (value: string | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onInput(term: string): void {
    this.display = term;
    if (this.value !== null) {
      this.value = null;
      this.selectedName = '';
      this.onChange(null);
    }
    this.openPanel();
    this.terms.next(term);
  }

  onFocus(): void {
    this.openPanel();
    this.terms.next('');
  }

  onBlur(): void {
    this.onTouched();
    this.closePanel();
  }

  onKeydown(event: KeyboardEvent): void {
    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        if (!this.open()) {
          this.onFocus();
        } else if (this.options().length) {
          this.activeIndex.set(Math.min(this.activeIndex() + 1, this.options().length - 1));
        }
        break;
      case 'ArrowUp':
        event.preventDefault();
        if (this.options().length) this.activeIndex.set(Math.max(this.activeIndex() - 1, 0));
        break;
      case 'Enter': {
        const active = this.options()[this.activeIndex()];
        if (this.open() && active) {
          event.preventDefault();
          this.select(active);
        }
        break;
      }
      case 'Escape':
        this.closePanel();
        break;
    }
  }

  select(option: AutocompleteOption): void {
    this.value = option.id;
    this.selectedName = option.name;
    this.display = option.name;
    this.onChange(option.id);
    this.onTouched();
    this.selected.emit(option);
    this.open.set(false);
  }

  clear(): void {
    this.value = null;
    this.selectedName = '';
    this.display = '';
    this.onChange(null);
    this.onTouched();
    this.open.set(false);
  }

  @HostListener('document:click', ['$event.target'])
  onDocumentClick(target: HTMLElement): void {
    if (this.open() && !this.host.nativeElement.contains(target)) this.closePanel();
  }

  private openPanel(): void {
    if (!this.open()) {
      this.options.set([]);
      this.loading.set(true);
      this.updateMenuPosition();
      this.open.set(true);
    }
  }

  private reposition = (): void => {
    if (this.open()) this.updateMenuPosition();
  };

  private updateMenuPosition(): void {
    const input = this.host.nativeElement.querySelector('input');
    if (!input) return;
    const rect = input.getBoundingClientRect();
    const gap = 2;
    const margin = 8;
    const spaceBelow = window.innerHeight - rect.bottom - margin;
    const spaceAbove = rect.top - margin;
    const openUp = spaceBelow < 180 && spaceAbove > spaceBelow;
    this.menuPos.set(
      openUp
        ? { top: null, bottom: window.innerHeight - rect.top + gap, left: rect.left, width: rect.width, maxHeight: Math.min(280, spaceAbove) }
        : { top: rect.bottom + gap, bottom: null, left: rect.left, width: rect.width, maxHeight: Math.min(280, spaceBelow) },
    );
  }

  private closePanel(): void {
    this.open.set(false);
    this.display = this.value ? this.selectedName : '';
  }

  private query(term: string): Observable<AutocompleteOption[]> {
    if (this.search) return this.search(term);
    const input = { filter: term || undefined, maxResultCount: 20, skipCount: 0 };
    const toOptions = map((res: { items?: { id?: string; name?: string }[] | null }) =>
      (res.items ?? []).map(i => ({ id: i.id!, name: i.name ?? '' })),
    );
    switch (this.kind) {
      case 'product':
        return this.productService.getList(input).pipe(toOptions);
      case 'category':
        return this.categoryService.getList(input).pipe(toOptions);
      case 'supplier':
        return this.supplierService.getList(input).pipe(toOptions);
      case 'warehouse':
        return this.warehouseService.getList(input).pipe(toOptions);
      default:
        return of([]);
    }
  }

  private resolveName(id: string | null): string {
    if (!id) return '';
    if (this.kind) {
      const name = this.lookup.nameOf(this.kind, id);
      return name === '—' ? '' : name;
    }
    return this.options().find(o => o.id === id)?.name ?? this.initialLabel;
  }
}
