import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '(document:keydown.escape)': 'onEscape()',
  },
})
export class ConfirmDialogComponent {
  readonly isOpen = input<boolean>(false);
  readonly title = input.required<string>();
  readonly message = input.required<string>();
  readonly confirmLabel = input<string>('ยืนยัน');
  readonly cancelLabel = input<string>('ยกเลิก');
  readonly tone = input<'primary' | 'danger'>('primary');
  readonly isSubmitting = input<boolean>(false);

  readonly confirmed = output<void>();
  readonly cancelled = output<void>();

  protected onConfirm(): void {
    if (this.isSubmitting()) {
      return;
    }
    this.confirmed.emit();
  }

  protected onCancel(): void {
    if (this.isSubmitting()) {
      return;
    }
    this.cancelled.emit();
  }

  protected onEscape(): void {
    if (!this.isOpen() || this.isSubmitting()) {
      return;
    }
    this.cancelled.emit();
  }
}
