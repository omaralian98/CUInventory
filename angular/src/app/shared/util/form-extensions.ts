import { AbstractControl, FormArray, FormControl, FormGroup } from '@angular/forms';

// Small casts to keep reactive-form templates type-safe.
export const getField = (c: AbstractControl | null): FormControl => c as FormControl;
export const getGroup = (c: AbstractControl | null): FormGroup => c as FormGroup;
export const getArray = (c: AbstractControl | null): FormArray => c as FormArray;
