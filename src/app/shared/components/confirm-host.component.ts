import { Component, inject } from '@angular/core';
import { ConfirmService } from '@core/services/confirm.service';

@Component({
  selector: 'sq-confirm-host',
  template: `
    @if (confirm.request(); as req) {
      <div class="dialog-overlay" (click)="confirm.respond(false)">
        <div class="dialog" (click)="$event.stopPropagation()">
          <div class="dialog-head">{{ req.title }}</div>
          <div class="dialog-body">{{ req.message }}</div>
          <div class="dialog-actions">
            <button class="btn btn-ghost" (click)="confirm.respond(false)">Cancel</button>
            <button class="btn" [class.btn-danger]="req.danger" [class.btn-primary]="!req.danger"
                    (click)="confirm.respond(true)">
              {{ req.confirmLabel ?? 'Confirm' }}
            </button>
          </div>
        </div>
      </div>
    }
  `
})
export class ConfirmHostComponent {
  readonly confirm = inject(ConfirmService);
}