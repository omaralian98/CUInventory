import { Component, EventEmitter, HostListener, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LocalizationPipe } from '@abp/ng.core';

/** Lightweight Bootstrap-styled modal driven by an `open` flag. Hosts every form + detail drawer. */
@Component({
  selector: 'cu-modal',
  standalone: true,
  imports: [CommonModule, LocalizationPipe],
  templateUrl: './modal.component.html',
  styleUrls: ['./modal.component.scss'],
})
export class ModalComponent {
  @Input() open = false;
  @Input() title = '';
  @Input() size: 'md' | 'lg' | 'xl' = 'md';
  @Input() busy = false;
  @Output() closed = new EventEmitter<void>();

  close(): void {
    if (this.busy) return;
    this.closed.emit();
  }

  @HostListener('document:keydown.escape')
  onEsc(): void {
    if (this.open) this.close();
  }
}
