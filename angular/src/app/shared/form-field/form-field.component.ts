import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { LocalizationPipe } from '@abp/ng.core';
import { getField } from '../util/form-extensions';
import { SelectOption } from '../enums/enum-labels';

/** Labeled reactive-form control (text/number/date/textarea/select) with validation display. */
@Component({
  selector: 'cu-form-field',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LocalizationPipe],
  templateUrl: './form-field.component.html',
})
export class FormFieldComponent {
  @Input({ required: true }) formGroup!: FormGroup;
  @Input({ required: true }) controlName!: string;
  @Input({ required: true }) label!: string;
  @Input() type: 'text' | 'number' | 'date' | 'textarea' | 'select' | 'checkbox' = 'text';
  @Input() placeholder = '';
  @Input() options: SelectOption[] = [];
  @Input() required = false;
  @Input() step?: number;
  @Input() min?: number;
  @Input() hint?: string;

  getField = getField;

  get control() {
    return this.formGroup.get(this.controlName);
  }

  get invalid(): boolean {
    const c = this.control;
    return !!c && c.invalid && (c.dirty || c.touched);
  }
}
