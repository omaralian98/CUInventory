import { RestService, Rest } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StockNotificationsService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  stream = (cancellationToken: any, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'GET',
      url: '/api/inventory/stock-notifications/stream',
    },
    { apiName: this.apiName,...config });
}