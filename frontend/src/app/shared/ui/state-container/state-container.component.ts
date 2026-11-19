import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

@Component({
  selector: 'app-state-container',
  standalone: true,
  templateUrl: './state-container.component.html',
  styleUrl: './state-container.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StateContainerComponent {
  readonly isLoading = input<boolean>(false);
  readonly isEmpty = input<boolean>(false);
  readonly errorMessage = input<string | null>(null);
  readonly errorTitle = input<string>('ไม่สามารถโหลดข้อมูลได้');
  readonly emptyTitle = input<string>('ไม่พบข้อมูล');
  readonly emptyDescription = input<string>('ยังไม่มีรายการที่ตรงกับเงื่อนไขในขณะนี้');
  readonly skeletonRows = input<number>(5);

  readonly retry = output<void>();

  protected readonly skeletonRowIndexes = computed(() => {
    const count = Math.max(1, this.skeletonRows());
    const rows: number[] = [];
    for (let index = 0; index < count; index++) {
      rows.push(index);
    }
    return rows;
  });

  protected onRetry(): void {
    this.retry.emit();
  }
}
