import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

/** Compact prev/next pager for server-paged tables. Hidden when everything fits on one page. */
@Component({
  selector: 'cu-pager',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (count > pageSize) {
      <div class="d-flex justify-content-end align-items-center gap-2 mt-3">
        <span class="cu-muted small">Page {{ page + 1 }} of {{ lastPage + 1 }}</span>
        <button type="button" class="btn btn-sm btn-outline-secondary" [disabled]="page <= 0" (click)="goTo(page - 1)"><i class="fas fa-angle-left"></i></button>
        <button type="button" class="btn btn-sm btn-outline-secondary" [disabled]="page >= lastPage" (click)="goTo(page + 1)"><i class="fas fa-angle-right"></i></button>
      </div>
    }
  `,
})
export class PagerComponent {
  @Input() page = 0;
  @Input() count = 0;
  @Input() pageSize = 20;
  @Output() pageChange = new EventEmitter<number>();

  get lastPage(): number {
    return Math.max(0, Math.ceil(this.count / this.pageSize) - 1);
  }

  goTo(page: number): void {
    this.pageChange.emit(Math.max(0, Math.min(page, this.lastPage)));
  }
}
