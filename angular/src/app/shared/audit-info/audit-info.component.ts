import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LocalizationPipe } from '@abp/ng.core';
import { UserLookupService } from '../lookup/user-lookup.service';

export interface AuditedEntity {
  creationTime?: string;
  creatorId?: string | null;
  lastModificationTime?: string | null;
  lastModifierId?: string | null;
}

/** Quiet audit footer for detail views: who created/last modified the row and when. */
@Component({
  selector: 'cu-audit-info',
  standalone: true,
  imports: [CommonModule, LocalizationPipe],
  template: `
    @if (entity) {
      <div class="cu-audit">
        <div class="cu-audit-item">
          <span class="cu-audit-icon"><i class="fas fa-circle-plus"></i></span>
          <div>
            <span class="cu-audit-label">{{ '::Created' | abpLocalization }}</span>
            <span class="cu-audit-value">
              {{ users.nameOf(entity.creatorId) }} · {{ entity.creationTime | date: 'short' }}
            </span>
          </div>
        </div>
        @if (entity.lastModificationTime) {
          <div class="cu-audit-item">
            <span class="cu-audit-icon"><i class="fas fa-pen"></i></span>
            <div>
              <span class="cu-audit-label">{{ '::LastModified' | abpLocalization }}</span>
              <span class="cu-audit-value">
                {{ users.nameOf(entity.lastModifierId) }} · {{ entity.lastModificationTime | date: 'short' }}
              </span>
            </div>
          </div>
        }
      </div>
    }
  `,
  styles: [
    `
      .cu-audit {
        display: flex;
        flex-wrap: wrap;
        gap: 0.5rem 2rem;
        margin-top: 1rem;
        padding-top: 0.75rem;
        border-top: 1px solid var(--cu-border);
      }
      .cu-audit-item {
        display: flex;
        align-items: center;
        gap: 0.6rem;
      }
      .cu-audit-icon {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: 1.9rem;
        height: 1.9rem;
        border-radius: 50%;
        background: var(--cu-surface-muted);
        color: var(--cu-text-muted);
        font-size: 0.75rem;
      }
      .cu-audit-label {
        display: block;
        font-size: 0.68rem;
        font-weight: 600;
        letter-spacing: 0.04em;
        text-transform: uppercase;
        color: var(--cu-text-muted);
      }
      .cu-audit-value {
        font-size: 0.82rem;
        color: var(--cu-text);
      }
    `,
  ],
})
export class AuditInfoComponent {
  @Input({ required: true }) entity: AuditedEntity | null = null;

  users = inject(UserLookupService);
}
