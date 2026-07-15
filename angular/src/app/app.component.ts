import { Component, inject } from '@angular/core';
import { DynamicLayoutComponent } from '@abp/ng.core';
import { LoaderBarComponent } from '@abp/ng.theme.shared';
import { OAuthService } from 'angular-oauth2-oidc';
import { StockNotificationStore } from './shared/realtime/stock-notification.store';

@Component({
  selector: 'app-root',
  template: `
    <abp-loader-bar />
    <abp-dynamic-layout />
  `,
  imports: [LoaderBarComponent, DynamicLayoutComponent],
})
export class AppComponent {
  private oauth = inject(OAuthService);
  private stockStore = inject(StockNotificationStore);

  constructor() {
    // Subscribe app-wide so low-stock toasts surface on any page, not just the dashboard.
    if (this.oauth.hasValidAccessToken()) {
      this.stockStore.start();
    }
    this.oauth.events.subscribe(e => {
      if (e.type === 'token_received' && this.oauth.hasValidAccessToken()) {
        this.stockStore.start();
      }
    });
  }
}
