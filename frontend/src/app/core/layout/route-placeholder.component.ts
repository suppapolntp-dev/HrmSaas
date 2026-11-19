import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-route-placeholder',
  standalone: true,
  template: `
    <section class="surface-card placeholder-box">
      <h1 class="placeholder-title">{{ pageTitle }}</h1>
      <p class="placeholder-desc">{{ pageDescription }}</p>
    </section>
  `,
  styles: [
    `
      .placeholder-box {
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
      }

      .placeholder-title {
        margin: 0;
        font-size: var(--text-lg);
        font-weight: var(--font-weight-semibold);
        color: var(--text-primary);
      }

      .placeholder-desc {
        margin: 0;
        font-size: var(--text-sm);
        color: var(--text-secondary);
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RoutePlaceholderComponent {
  private readonly route = inject(ActivatedRoute);
  protected readonly pageTitle: string =
    (this.route.snapshot.data['title'] as string | undefined) ?? 'hrm';
  protected readonly pageDescription: string =
    (this.route.snapshot.data['description'] as string | undefined) ??
    'หน้านี้เตรียม Route และ Guard ไว้แล้ว พร้อมสำหรับพัฒนาฟีเจอร์ในขั้นถัดไป';
}
