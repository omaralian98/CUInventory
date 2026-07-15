import { Injectable, inject } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { environment } from '../../../environments/environment';
import { StockNotificationDto, StockNotificationEventName } from './stock-notification.model';

export interface StockStreamHandlers {
  onEvent: (name: StockNotificationEventName, payload: StockNotificationDto) => void;
  onConnected?: () => void;
  onError?: (err: unknown) => void;
}

/**
 * Consumes the .NET 10 SSE stream. EventSource can't carry the OAuth bearer token, so
 * we read the stream with fetch + ReadableStream and parse SSE frames ourselves. The
 * frame `event:` name is the notification type (StockChanged | LowStockReached).
 */
@Injectable({ providedIn: 'root' })
export class StockNotificationClient {
  private oauth = inject(OAuthService);
  private readonly url = `${environment.apis.default.url}/api/inventory/stock-notifications/stream`;

  /** Opens the stream; returns a disposer that aborts it. */
  connect(handlers: StockStreamHandlers): () => void {
    const controller = new AbortController();
    this.run(controller, handlers);
    return () => controller.abort();
  }

  private async run(controller: AbortController, handlers: StockStreamHandlers): Promise<void> {
    try {
      const token = this.oauth.getAccessToken();
      const response = await fetch(this.url, {
        headers: {
          Accept: 'text/event-stream',
          ...(token ? { Authorization: `Bearer ${token}` } : {}),
        },
        signal: controller.signal,
      });

      if (!response.ok || !response.body) {
        handlers.onError?.(new Error(`Stream failed: ${response.status}`));
        return;
      }

      handlers.onConnected?.();
      const reader = response.body.getReader();
      const decoder = new TextDecoder();
      let buffer = '';

      while (true) {
        const { value, done } = await reader.read();
        if (done) break;
        buffer += decoder.decode(value, { stream: true });

        // Frames are separated by a blank line.
        let sep: number;
        while ((sep = buffer.indexOf('\n\n')) >= 0) {
          const frame = buffer.slice(0, sep);
          buffer = buffer.slice(sep + 2);
          this.dispatchFrame(frame, handlers);
        }
      }

      // A clean server close (restart, proxy timeout) must trigger a reconnect too.
      if (!controller.signal.aborted) {
        handlers.onError?.(new Error('Stream closed by server'));
      }
    } catch (err) {
      if (!controller.signal.aborted) handlers.onError?.(err);
    }
  }

  private dispatchFrame(frame: string, handlers: StockStreamHandlers): void {
    let event: string | null = null;
    const dataLines: string[] = [];
    for (const line of frame.split('\n')) {
      if (line.startsWith('event:')) event = line.slice(6).trim();
      else if (line.startsWith('data:')) dataLines.push(line.slice(5).trim());
    }
    if (dataLines.length === 0) return;

    try {
      const payload = JSON.parse(dataLines.join('\n')) as StockNotificationDto;
      const name = (event ?? 'StockChanged') as StockNotificationEventName;
      handlers.onEvent(name, payload);
    } catch {
      // Ignore keep-alive / malformed frames.
    }
  }
}
