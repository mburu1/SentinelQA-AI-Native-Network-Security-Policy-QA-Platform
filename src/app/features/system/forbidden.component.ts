import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'sq-forbidden',
  imports: [RouterLink],
  template: `
    <div class="login-shell">
      <div class="login-card" style="text-align:center">
        <div style="font-size:44px">🔒</div>
        <h2>403 — Access denied</h2>
        <p class="muted">Your role does not grant access to this area. This attempt has been audited.</p>
        <a class="btn btn-primary" routerLink="/dashboard">Back to dashboard</a>
      </div>
    </div>
  `
})
export class ForbiddenComponent {}