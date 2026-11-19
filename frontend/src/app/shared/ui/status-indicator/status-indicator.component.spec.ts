import { TestBed } from '@angular/core/testing';
import { StatusIndicatorComponent } from './status-indicator.component';

describe('StatusIndicatorComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StatusIndicatorComponent],
    }).compileComponents();
  });

  it('should render semantic dot and label without badge classes', () => {
    const fixture = TestBed.createComponent(StatusIndicatorComponent);
    fixture.componentRef.setInput('label', 'อนุมัติแล้ว');
    fixture.componentRef.setInput('tone', 'success');
    fixture.detectChanges();

    const el = fixture.nativeElement as HTMLElement;
    const indicator = el.querySelector('.status-indicator');
    const dot = el.querySelector('.status-dot');

    expect(indicator?.classList.contains('tone-success')).toBe(true);
    expect(dot).not.toBeNull();
    expect(el.textContent).toContain('อนุมัติแล้ว');
    expect(el.querySelector('.badge, .pill')).toBeNull();
  });
});
