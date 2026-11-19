import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { StateContainerComponent } from './state-container.component';

@Component({
  standalone: true,
  imports: [StateContainerComponent],
  template: `
    <app-state-container
      [isLoading]="isLoading()"
      [isEmpty]="isEmpty()"
      [errorMessage]="errorMessage()"
      (retry)="retried = true"
    >
      <div class="real-content">Loaded Data</div>
    </app-state-container>
  `,
})
class TestHostComponent {
  readonly isLoading = signal(false);
  readonly isEmpty = signal(false);
  readonly errorMessage = signal<string | null>(null);
  retried = false;
}

describe('StateContainerComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestHostComponent],
    }).compileComponents();
  });

  it('should switch between loading, error with retry, empty, and default content', () => {
    const fixture = TestBed.createComponent(TestHostComponent);
    const host = fixture.componentInstance;

    host.isLoading.set(true);
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).querySelector('.skeleton-container')).not.toBeNull();

    host.isLoading.set(false);
    host.errorMessage.set('API ถูกปิดสำหรับการทดสอบ');
    fixture.detectChanges();
    const retryBtn = (fixture.nativeElement as HTMLElement).querySelector('button');
    expect(retryBtn?.textContent).toContain('ลองใหม่อีกครั้ง');
    retryBtn?.click();
    expect(host.retried).toBe(true);

    host.errorMessage.set(null);
    host.isEmpty.set(true);
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).querySelector('.empty-state')).not.toBeNull();

    host.isEmpty.set(false);
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).querySelector('.real-content')?.textContent).toContain(
      'Loaded Data',
    );
  });
});
