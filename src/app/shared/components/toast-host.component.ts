import { NgClass } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ToastService } from '@core/services/toast.service';

@Component({
  selector: 'sq-toast-host',
  imports: [NgClass],
  template: `
    <div class="toast-host">
      @for (t of toast.toasts(); track t.id) {
        <div class="toast" [ngClass]="'toast-' + t.kind" (click)="toast.dismiss(t.id)">
          <span>{{ icon(t.kind) }}</span>
          <span>{{ t.message }}</span>
        </div>
      }
    </div>
  `
})
export class ToastHostComponent {
  readonly toast = inject(ToastService);

  icon(kind: string): string {
    return { success: '✅', error: '⛔', warning: '⚠️', info: 'ℹ️' }[kind] ?? 'ℹ️';
  }
}