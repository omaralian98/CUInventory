import { Injectable, inject, signal } from '@angular/core';
import { PermissionService, RestService } from '@abp/ng.core';

/**
 * Resolves audit user ids (creatorId, lastModifierId) to usernames via the identity
 * API, one cached request per id. Viewers without identity permission — and ids of
 * deleted users — fall back to a shortened id so the audit strip always renders.
 */
@Injectable({ providedIn: 'root' })
export class UserLookupService {
  private rest = inject(RestService);
  private permissions = inject(PermissionService);

  private names = signal<Record<string, string>>({});
  private requested = new Set<string>();

  nameOf(id: string | null | undefined): string {
    if (!id) return '—';
    const cached = this.names()[id];
    if (cached) return cached;
    this.fetch(id);
    return this.shortId(id);
  }

  private fetch(id: string): void {
    if (this.requested.has(id)) return;
    this.requested.add(id);

    if (!this.permissions.getGrantedPolicy('AbpIdentity.Users')) {
      this.cache(id, this.shortId(id));
      return;
    }

    this.rest
      .request<void, { userName?: string }>(
        { method: 'GET', url: `/api/identity/users/${id}` },
        { apiName: 'Default', skipHandleError: true },
      )
      .subscribe({
        next: user => this.cache(id, user?.userName || this.shortId(id)),
        error: () => this.cache(id, this.shortId(id)),
      });
  }

  private cache(id: string, name: string): void {
    this.names.update(map => ({ ...map, [id]: name }));
  }

  private shortId(id: string): string {
    return id.slice(0, 8);
  }
}
