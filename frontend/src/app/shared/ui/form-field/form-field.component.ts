import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  input,
  signal,
} from '@angular/core';
import { AbstractControl } from '@angular/forms';

@Component({
  selector: 'app-form-field',
  standalone: true,
  templateUrl: './form-field.component.html',
  styleUrl: './form-field.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FormFieldComponent {
  readonly label = input.required<string>();
  readonly forId = input<string>('');
  readonly required = input<boolean>(false);
  readonly hint = input<string | null>(null);
  readonly errorMessage = input<string | null>(null);
  readonly control = input<AbstractControl | null>(null);

  private readonly controlStateVersion = signal(0);

  constructor() {
    effect((onCleanup) => {
      const targetControl = this.control();
      if (!targetControl) {
        return;
      }

      const subscription = targetControl.events.subscribe(() => {
        this.controlStateVersion.update((version) => version + 1);
      });

      onCleanup(() => {
        subscription.unsubscribe();
      });
    });
  }

  protected readonly displayedError = computed<string | null>(() => {
    this.controlStateVersion();

    const explicitError = this.errorMessage();
    if (explicitError && explicitError.trim().length > 0) {
      return explicitError;
    }

    const targetControl = this.control();
    if (!targetControl || !targetControl.invalid) {
      return null;
    }

    if (!targetControl.touched && !targetControl.dirty) {
      return null;
    }

    const errors = targetControl.errors;
    if (!errors) {
      return null;
    }

    if (typeof errors['serverError'] === 'string') {
      return errors['serverError'];
    }
    if (errors['required']) {
      return 'กรุณากรอกข้อมูลในช่องนี้';
    }
    if (errors['email']) {
      return 'รูปแบบอีเมลไม่ถูกต้อง';
    }
    if (errors['minlength']) {
      const minLength = errors['minlength'] as { requiredLength: number };
      return `กรุณากรอกอย่างน้อย ${minLength.requiredLength} ตัวอักษร`;
    }
    if (errors['maxlength']) {
      const maxLength = errors['maxlength'] as { requiredLength: number };
      return `กรุณากรอกไม่เกิน ${maxLength.requiredLength} ตัวอักษร`;
    }
    if (errors['min']) {
      const minRule = errors['min'] as { min: number };
      return `ค่าต้องไม่น้อยกว่า ${minRule.min}`;
    }
    if (errors['max']) {
      const maxRule = errors['max'] as { max: number };
      return `ค่าต้องไม่เกิน ${maxRule.max}`;
    }

    return 'ข้อมูลไม่ถูกต้อง';
  });
}
