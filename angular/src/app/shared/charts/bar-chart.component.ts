import { Component, Input, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface BarDatum {
  label: string;
  value: number;
}

/**
 * Horizontal magnitude bars (single hue — this compares magnitudes, identity is the
 * row label). Direct value labels satisfy the low-contrast-fill relief rule.
 * Dependency-free HTML/CSS; responsive.
 */
@Component({
  selector: 'cu-bar-chart',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './bar-chart.component.html',
  styleUrls: ['./bar-chart.component.scss'],
})
export class BarChartComponent {
  private _data = signal<BarDatum[]>([]);
  @Input({ required: true }) set data(v: BarDatum[]) {
    this._data.set(v ?? []);
  }
  @Input() unit = '';
  @Input() emptyText = 'No data for the selected filters.';

  rows = computed(() => {
    const data = this._data();
    const max = Math.max(1, ...data.map(d => Math.abs(d.value)));
    return data.map(d => ({
      label: d.label,
      value: d.value,
      pct: (Math.abs(d.value) / max) * 100,
    }));
  });

  format(n: number): string {
    return new Intl.NumberFormat(undefined, { maximumFractionDigits: 2 }).format(n);
  }
}
