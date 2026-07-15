import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
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
  imports: [RouterLink, PageShellComponent],
  templateUrl: './reports-hub.component.html',
  styleUrls: ['./reports-hub.component.scss'],
})
export class ReportsHubComponent {
  cards: ReportCard[] = [
    { route: 'sales-by-source', title: 'Sales by Source', description: 'How much of each product sold came from which supplier, with revenue, cost and gross margin.', icon: 'fa-arrow-trend-up', tone: 'info' },
    { route: 'remaining-stock', title: 'Remaining Stock', description: 'On-hand stock grouped by warehouse and originating supplier, valued at cost. Trace a shipment’s leftovers.', icon: 'fa-boxes-stacked', tone: 'success' },
    { route: 'inventory-valuation', title: 'Inventory Valuation', description: 'Total quantity and value on hand, grouped by warehouse and category.', icon: 'fa-scale-balanced', tone: 'neutral' },
    { route: 'low-stock', title: 'Low Stock', description: 'Balances at or below their configured low-stock threshold.', icon: 'fa-triangle-exclamation', tone: 'warning' },
  ];
}
