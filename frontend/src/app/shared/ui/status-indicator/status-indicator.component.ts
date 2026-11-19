import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

export type StatusTone = 'success' | 'warning' | 'danger' | 'info' | 'neutral';

@Component({
  selector: 'app-status-indicator',
  standalone: true,
  templateUrl: './status-indicator.component.html',
  styleUrl: './status-indicator.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatusIndicatorComponent {
  readonly label = input.required<string>();
  readonly tone = input<StatusTone>('neutral');
  readonly hideDot = input<boolean>(false);

  protected readonly toneClass = computed(() => {
    switch (this.tone()) {
      case 'success':
        return 'tone-success';
      case 'warning':
        return 'tone-warning';
      case 'danger':
        return 'tone-danger';
      case 'info':
        return 'tone-info';
      default:
        return 'tone-neutral';
    }
  });
}
