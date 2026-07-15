import { Component, Input, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LocalizationPipe, LocalizationService } from '@abp/ng.core';

export interface DonutDatum {
  label: string;
  value: number;
}

interface Segment {
  label: string;
  value: number;
  pct: number;
  color: string;
  dash: number;
  offset: number;
}

const SERIES = [
  'var(--cu-series-1)',
  'var(--cu-series-2)',
  'var(--cu-series-3)',
  'var(--cu-series-4)',
  'var(--cu-series-5)',
  'var(--cu-series-6)',
];

/**
 * Donut for categorical share (identity is the point → categorical palette, in fixed
 * order, never cycled). A 7th+ category folds into "Other". Legend carries identity;
 * center shows the total. Dependency-free SVG.
 */
@Component({
  selector: 'cu-donut-chart',
  standalone: true,
  imports: [CommonModule, LocalizationPipe],
  templateUrl: './donut-chart.component.html',
  styleUrls: ['./donut-chart.component.scss'],
})
export class DonutChartComponent {
  private localization = inject(LocalizationService);
  private _data = signal<DonutDatum[]>([]);
  @Input({ required: true }) set data(v: DonutDatum[]) {
    this._data.set(v ?? []);
  }
  @Input() centerLabel = '::Total';
  @Input() emptyText = '::NoChartData';

  readonly radius = 60;
  readonly circumference = 2 * Math.PI * this.radius;

  private prepared = computed(() => {
    const raw = [...this._data()].filter(d => d.value > 0).sort((a, b) => b.value - a.value);
    if (raw.length <= SERIES.length) return raw;
    // Fold the long tail into a single "Other" slice — never invent a new hue.
    const head = raw.slice(0, SERIES.length - 1);
    const tail = raw.slice(SERIES.length - 1);
    const other = tail.reduce((sum, d) => sum + d.value, 0);
    return [...head, { label: this.localization.instant('::Other'), value: other }];
  });

  total = computed(() => this.prepared().reduce((sum, d) => sum + d.value, 0));

  segments = computed<Segment[]>(() => {
    const items = this.prepared();
    const total = this.total() || 1;
    const gap = 2; // px surface gap between slices
    let cursor = 0;
    return items.map((d, i) => {
      const pct = (d.value / total) * 100;
      const len = (d.value / total) * this.circumference;
      const dash = Math.max(0, len - gap);
      const seg: Segment = {
        label: d.label,
        value: d.value,
        pct,
        color: SERIES[i % SERIES.length],
        dash,
        offset: -cursor,
      };
      cursor += len;
      return seg;
    });
  });

  formatTotal(): string {
    return new Intl.NumberFormat(undefined, { maximumFractionDigits: 0 }).format(this.total());
  }

  formatPct(pct: number): string {
    return pct.toFixed(pct < 10 ? 1 : 0) + '%';
  }
}
