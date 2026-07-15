import { FormArray, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ProductService } from '../../proxy/catalog/product.service';
import { AutocompleteOption } from '../autocomplete/autocomplete.component';

export function takenProductIds(lines: FormArray, current?: FormGroup): Set<string> {
  const taken = new Set<string>();
  for (const group of lines.controls) {
    if (group === current) continue;
    const id = group.get('productId')?.value as string | null;
    if (id) taken.add(id);
  }
  return taken;
}

export function productLineSearch(
  products: ProductService,
  lines: () => FormArray,
): (group: FormGroup) => (term: string) => Observable<AutocompleteOption[]> {
  return group => term => {
    const taken = takenProductIds(lines(), group);
    return products.getList({ filter: term || undefined, maxResultCount: 20, skipCount: 0 }).pipe(
      map(res =>
        (res.items ?? [])
          .filter(p => !taken.has(p.id!))
          .map(p => ({ id: p.id!, name: p.name ?? '' })),
      ),
    );
  };
}
