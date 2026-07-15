import { Pipe, PipeTransform, inject } from '@angular/core';
import { LookupKind, LookupService } from './lookup.service';

/**
 * Resolves a Guid to its entity name, e.g. {{ row.productId | idName: 'product' }}.
 * Impure so it reflects the reference lists once they finish loading (table pages are
 * small and paged, so the cost is negligible).
 */
@Pipe({ name: 'idName', standalone: true, pure: false })
export class IdNamePipe implements PipeTransform {
  private lookup = inject(LookupService);

  transform(id: string | null | undefined, kind: LookupKind): string {
    this.lookup.load();
    return this.lookup.nameOf(kind, id);
  }
}
