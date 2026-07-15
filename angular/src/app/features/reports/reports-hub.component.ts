import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LocalizationPipe } from '@abp/ng.core';
import { PageShellComponent } from '../../shared';

interface ReportCard {
  route: string;
  title: string;
  description: string;
  icon: string;
  tone: string;
}

@Component({
  selector: 'cu-reports-hub',
  standalone: true,
  imports: [RouterLink, LocalizationPipe, PageShellComponent],
  templateUrl: './reports-hub.component.html',
  styleUrls: ['./reports-hub.component.scss'],
})
export class ReportsHubComponent {
  cards: ReportCard[] = [
    { route: 'sales-by-source', title: '::Reports:SalesBySource', description: '::Reports:SalesBySourceDesc', icon: 'fa-arrow-trend-up', tone: 'info' },
    { route: 'remaining-stock', title: '::Reports:RemainingStock', description: '::Reports:RemainingStockDesc', icon: 'fa-boxes-stacked', tone: 'success' },
    { route: 'inventory-valuation', title: '::Reports:InventoryValuation', description: '::Reports:InventoryValuationDesc', icon: 'fa-scale-balanced', tone: 'neutral' },
    { route: 'low-stock', title: '::Reports:LowStock', description: '::Reports:LowStockDesc', icon: 'fa-triangle-exclamation', tone: 'warning' },
  ];
}
