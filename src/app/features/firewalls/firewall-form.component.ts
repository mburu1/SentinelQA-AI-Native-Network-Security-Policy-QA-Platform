import { Component, DestroyRef, inject, input, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FirewallsService } from '@core/services/firewalls.service';
import { ToastService } from '@core/services/toast.service';
import { ApiError, ENVIRONMENTS, FIREWALL_VENDORS } from '@core/models';
import { PageHeaderComponent } from '@shared/components/page-header.component';

@Component({
  selector: 'sq-firewall-form',
  imports: [ReactiveFormsModule, RouterLink, PageHeaderComponent],
  template: `
    <sq-page-header [title]="id() ? 'Edit Firewall' : 'Register Firewall'"
                    subtitle="Firewall inventory entry used by policy management and deployments" />

    @if (error(); as err) { <div class="alert alert-error">{{ err }}</div> }

    <div class="card"><div class="card-body" style="max-width:640px">
      <form [formGroup]="form" (ngSubmit)="submit()">
        <div class="form-field">
          <label class="label">Name</label>
          <input class="input" formControlName="name" placeholder="edge-fw-prod-01" />
        </div>
        <div class="form-grid">
          <div class="form-field">
            <label class="label">Vendor</label>
            <select class="select" formControlName="vendor">
              @for (v of vendors; track v) { <option [value]="v">{{ v }}</option> }
            </select>
          </div>
          <div class="form-field">
            <label class="label">Environment</label>
            <select class="select" formControlName="environment">
              @for (e of environments; track e) { <option [value]="e">{{ e }}</option> }
            </select>
          </div>
          <div class="form-field">
            <label class="label">IP Address</label>
            <input class="input mono" formControlName="ipAddress" placeholder="10.0.0.1" />
          </div>
          <div class="form-field">
            <label class="label">Model</label>
            <input class="input" formControlName="model" placeholder="PA-450" />
          </div>
        </div>
        <div style="display:flex;gap:10px;margin-top:8px">
          <button class="btn btn-primary" type="submit" [disabled]="busy()">
            {{ busy() ? 'Saving…' : (id() ? 'Save changes' : 'Register firewall') }}
          </button>
          <a class="btn btn-ghost" routerLink="/firewalls">Cancel</a>
        </div>
      </form>
    </div></div>
  `
})
export class FirewallFormComponent implements OnInit {
  readonly id = input<string>();

  private readonly svc = inject(FirewallsService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly vendors = FIREWALL_VENDORS;
  readonly environments = ENVIRONMENTS;
  readonly busy = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(3)] }),
    vendor: new FormControl<string>('Simulated', { nonNullable: true, validators: [Validators.required] }),
    environment: new FormControl<string>('QA', { nonNullable: true, validators: [Validators.required] }),
    ipAddress: new FormControl<string>(''),
    model: new FormControl<string>('')
  });

  ngOnInit(): void {
    const id = this.id();
    if (!id) return;
    this.svc.get(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: fw => this.form.patchValue({
        name: fw.name, vendor: fw.vendor, environment: fw.environment,
        ipAddress: fw.ipAddress ?? '', model: fw.model ?? ''
      }),
      error: () => this.error.set('Firewall not found.')
    });
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.busy.set(true);
    this.error.set(null);
    const payload = this.form.getRawValue();
    const id = this.id();

    const request$ = id ? this.svc.update(id, payload) : this.svc.create(payload);
    request$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: fw => {
        this.toast.success(`Firewall "${fw.name}" saved.`);
        this.router.navigateByUrl(`/firewalls/${fw.id}`);
      },
      error: (err: ApiError) => {
        this.busy.set(false);
        this.error.set(err.fieldErrors.join(' ') || err.detail || err.title);
      }
    });
  }
}