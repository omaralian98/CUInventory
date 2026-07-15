import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export const PHONE_REGEX = /^\+?(?:[\s\-()]*\d){7,15}[\s\-()]*$/;

export const phoneValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = control.value as string | null;
  if (!value) return null;
  return PHONE_REGEX.test(value) ? null : { phone: true };
};
