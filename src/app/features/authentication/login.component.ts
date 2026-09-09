import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '@core/auth/auth.service';
import { ApiError } from '@core/models';

@Component({
  selector: 'sq-login',
  imports: [ReactiveFormsModule],
  template: `
    <div class="login-shell">
      <div class="login-card">
        <div class="login-brand">🛡️ SentinelQA</div>
        <div class="login-sub">AI-Native Network Security Policy &amp; QA Platform</div>

        @if (error(); as err) {
          <div class="alert alert-error" data-testid="login-error">{{ err }}</div>
        }

        <form [formGroup]="form" (ngSubmit)="submit()">
          <div class="form-field">
            <label class="label" for="email">Email</label>
            <input id="email" class="input" formControlName="email" autocomplete="username" data-testid="login-email" />
          </div>
          <div class="form-field">
            <label class="label" for="password">Password</label>
            <input id="password" type="password" class="input" formControlName="password"
                   autocomplete="current-password" data-testid="login-password" />
          </div>
          <button class="btn btn-primary" style="width:100%;justify-content:center" type="submit"
                  [disabled]="busy()" data-testid="login-submit">
            {{ busy() ? 'Signing in…' : 'Sign in' }}
          </button>
        </form>

        <p class="muted small" style="margin-top:18px">
          Seeded users: <span class="mono">admin&#64;sentinelqa.local</span>,
          <span class="mono">qa&#64;sentinelqa.local</span>,
          <span class="mono">approver&#64;sentinelqa.local</span>
        </p>
      </div>
    </div>
  `
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly busy = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = new FormGroup({
    email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    password: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(6)] })
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.busy.set(true);
    this.error.set(null);
    const { email, password } = this.form.getRawValue();

    this.auth.login(email, password).subscribe({
      next: () => {
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/dashboard';
        this.router.navigateByUrl(returnUrl);
      },
      error: (err: ApiError) => {
        this.busy.set(false);
        this.error.set(err.detail ?? err.title ?? 'Login failed. Check your credentials.');
      }
    });
  }
}