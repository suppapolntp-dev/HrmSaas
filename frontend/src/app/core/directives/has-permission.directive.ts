import {
  Directive,
  TemplateRef,
  ViewContainerRef,
  effect,
  inject,
  input,
} from '@angular/core';
import { AuthSessionStore } from '../auth/auth-session.store';

@Directive({
  selector: '[appHasPermission]',
  standalone: true,
})
export class HasPermissionDirective {
  readonly appHasPermission = input.required<string>();

  private readonly templateRef = inject(TemplateRef<unknown>);
  private readonly viewContainer = inject(ViewContainerRef);
  private readonly authSessionStore = inject(AuthSessionStore);
  private isRendered = false;

  constructor() {
    effect(() => {
      const requiredPermission = this.appHasPermission();
      const allowed = this.authSessionStore.hasPermission(requiredPermission);

      if (allowed && !this.isRendered) {
        this.viewContainer.createEmbeddedView(this.templateRef);
        this.isRendered = true;
      } else if (!allowed && this.isRendered) {
        this.viewContainer.clear();
        this.isRendered = false;
      }
    });
  }
}
