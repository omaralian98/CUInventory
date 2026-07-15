import { Component, ContentChild, Input, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormGroup, ReactiveFormsModule } from '@angular/forms';

/** Directive marking the row-cell template: <ng-template cuLineRow let-group let-i="index">…<td>…</td> */
import { Directive, inject } from '@angular/core';
@Directive({ selector: 'ng-template[cuLineRow]', standalone: true })
export class LineRowDirective {
  readonly template = inject(TemplateRef);
}

/**
 * Repeating line-item editor over a FormArray. The parent supplies the per-row
 * FormGroup factory and a row template that emits the cell <td>s; this owns the
 * table chrome, add/remove buttons, and the "at least one line" guard.
 * Reused by Purchase Order, Shipment, Transfer and Sale forms.
 */
@Component({
  selector: 'cu-line-items',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './line-items.component.html',
  styleUrls: ['./line-items.component.scss'],
})
export class LineItemsComponent {
  @Input({ required: true }) formArray!: FormArray;
  @Input({ required: true }) headers: string[] = [];
  @Input({ required: true }) newRow!: () => FormGroup;
  @Input() addLabel = 'Add line';
  @Input() minRows = 1;

  @ContentChild(LineRowDirective) rowTemplate!: LineRowDirective;

  get groups(): FormGroup[] {
    return this.formArray.controls as FormGroup[];
  }

  addRow(): void {
    this.formArray.push(this.newRow());
  }

  removeRow(index: number): void {
    this.formArray.removeAt(index);
  }

  get canRemove(): boolean {
    return this.formArray.length > this.minRows;
  }
}
