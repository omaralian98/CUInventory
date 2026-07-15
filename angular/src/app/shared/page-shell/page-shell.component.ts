import { Component, Input } from '@angular/core';
import { LocalizationPipe } from '@abp/ng.core';

/**
 * Standard chrome for every feature page: a title bar with an actions slot and a
 * scrollable body. Pure layout — no business logic. Reused across the whole module.
 */
@Component({
  selector: 'cu-page-shell',
  standalone: true,
  imports: [LocalizationPipe],
  templateUrl: './page-shell.component.html',
  styleUrls: ['./page-shell.component.scss'],
})
export class PageShellComponent {
  @Input() title!: string;
  @Input() subtitle?: string;
}
