import { Directive, Input, TemplateRef, inject } from '@angular/core';

/** Lets a page override a cell's rendering: <ng-template cuCol="status" let-row>…</ng-template> */
@Directive({ selector: 'ng-template[cuCol]', standalone: true })
export class ColumnDirective {
  @Input('cuCol') prop!: string;
  readonly template = inject(TemplateRef);
}
