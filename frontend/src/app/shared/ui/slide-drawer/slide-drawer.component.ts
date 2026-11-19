import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'app-slide-drawer',
  standalone: true,
  templateUrl: './slide-drawer.component.html',
  styleUrl: './slide-drawer.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '(document:keydown.escape)': 'onEscape()',
  },
})
export class SlideDrawerComponent {
  readonly isOpen = input<boolean>(false);
  readonly title = input.required<string>();
  readonly subtitle = input<string | null>(null);
  readonly size = input<'md' | 'lg'>('md');
  readonly closeOnBackdrop = input<boolean>(true);

  readonly closed = output<void>();

  protected onClose(): void {
    this.closed.emit();
  }

  protected onBackdropClick(): void {
    if (!this.closeOnBackdrop()) {
      return;
    }
    this.closed.emit();
  }

  protected onEscape(): void {
    if (!this.isOpen()) {
      return;
    }
    this.closed.emit();
  }
}
