import { Directive, inject, signal } from '@angular/core';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { ConfirmationService, ToasterService, Confirmation } from '@abp/ng.theme.shared';
import { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';

/**
 * Common wiring for a list page: holds rows/count, hooks the ListService query, and
 * provides confirm + toast helpers around the lifecycle/CRUD calls. Feature pages
 * subclass this and supply their query. Keeps each page focused on its own columns/form.
 */
@Directive()
export abstract class ListPageBase<T> {
  readonly list = inject(ListService);
  protected readonly toaster = inject(ToasterService);
  protected readonly confirmation = inject(ConfirmationService);

  readonly rows = signal<T[]>([]);
  readonly count = signal(0);
  readonly loading = signal(false);

  /** Subclasses call this in their constructor with the proxy getList stream. */
  protected hook(streamFactory: (query: any) => Observable<PagedResultDto<T>>): void {
    this.loading.set(true);
    this.list.hookToQuery(streamFactory).subscribe(res => {
      this.rows.set(res.items ?? []);
      this.count.set(res.totalCount ?? 0);
      this.loading.set(false);
    });
  }

  protected reload(): void {
    this.list.get();
  }

  /** Confirm, then run an action; toast on success/failure and reload the list. */
  protected confirmAction(
    message: string,
    title: string,
    run: () => Observable<unknown>,
    successMessage: string,
  ): void {
    this.confirmation.warn(message, title).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.runAction(run, successMessage);
    });
  }

  protected runAction(run: () => Observable<unknown>, successMessage: string): void {
    this.loading.set(true);
    run()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: () => {
          this.toaster.success(successMessage);
          this.reload();
        },
        error: err => {
          this.toaster.error(err?.error?.error?.message ?? 'The operation failed.', 'Error');
        },
      });
  }
}
