import { Component, Input, computed, signal } from '@angular/core';
import { LocalizationPipe } from '@abp/ng.core';
import { EnumKind, enumEntry } from '../enums/enum-labels';

/** A colored chip for any status enum, fed an enum kind + its numeric value. */
@Component({
  selector: 'cu-status-badge',
  standalone: true,
  imports: [LocalizationPipe],
  template: `<span class="cu-chip" [class]="'cu-chip--' + entry().tone">{{ entry().label | abpLocalization }}</span>`,
})
export class StatusBadgeComponent {
  private _kind = signal<EnumKind>('sale-status');
  private _value = signal<number | null | undefined>(undefined);

  @Input({ required: true }) set kind(v: EnumKind) {
    this._kind.set(v);
  }
  @Input({ required: true }) set value(v: number | null | undefined) {
    this._value.set(v);
  }

  entry = computed(() => enumEntry(this._kind(), this._value()));
}
