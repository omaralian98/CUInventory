import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BadgeTone } from '../enums/enum-labels';

/** KPI tile: label, big value, optional hint + icon. Used on the dashboard and report headers. */
@Component({
  selector: 'cu-stat-tile',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="cu-tile" [class]="'cu-tile--' + tone">
      <div class="cu-tile-body">
        <span class="cu-tile-label">{{ label }}</span>
        <span class="cu-tile-value cu-mono">{{ value }}</span>
        @if (hint) { <span class="cu-tile-hint">{{ hint }}</span> }
      </div>
      @if (icon) {
        <span class="cu-tile-icon"><i class="fas {{ icon }}"></i></span>
      }
    </div>
  `,
  styleUrls: ['./stat-tile.component.scss'],
})
export class StatTileComponent {
  @Input({ required: true }) label!: string;
  @Input({ required: true }) value!: string | number;
  @Input() hint?: string;
  @Input() icon?: string;
  @Input() tone: BadgeTone = 'info';
}
