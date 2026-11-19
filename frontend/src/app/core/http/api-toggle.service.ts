import { Injectable, signal } from '@angular/core';

const API_TOGGLE_STORAGE_KEY = 'hrm_dev_api_enabled';

@Injectable({ providedIn: 'root' })
export class ApiToggleService {
  readonly isApiEnabled = signal<boolean>(this.readInitialState());

  toggleApi(): void {
    const nextValue = !this.isApiEnabled();
    this.isApiEnabled.set(nextValue);
    sessionStorage.setItem(API_TOGGLE_STORAGE_KEY, String(nextValue));
  }

  private readInitialState(): boolean {
    const saved = sessionStorage.getItem(API_TOGGLE_STORAGE_KEY);
    if (saved === 'false') {
      return false;
    }
    return true;
  }
}
