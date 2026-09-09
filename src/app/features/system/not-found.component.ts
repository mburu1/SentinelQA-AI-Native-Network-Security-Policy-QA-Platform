import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'sq-not-found',
  imports: [RouterLink],
  template: `
    <div class="login-shell">
      <div class="login-card" style="text-align:center">
        <div style="font-size:44px">🧭</div>
        <h2>404 — Page not found</h2>
        <p class="muted">The resource you are looking for does not exist or was moved.</p>
        <a class="btn btn-primary" routerLink="/dashboard">Back to dashboard</a>
      </div>
    </div>
  `
})
export class NotFoundComponent {}