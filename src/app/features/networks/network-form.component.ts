import { Component, DestroyRef, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NetworksService } from '@core/services/networks.service';
import { ToastService } from '@core/services/toast.service';
import { ApiError, NETWORK_TYPES, NetworkType } from '@core/models';
import { PageHeaderComponent } from '@shared/components/page-header.component';
import { cidrValidator } from '@shared/validators/security.validators';

@Component({
  selector: 'sq-network-form',
  imports: [ReactiveFormsModule, RouterLink, PageHeaderComponent],
  template: `
    <sq-page-header title="Define Network" subtitle="Address space usable as source/destination in policy rules" />

    @if (error(); as err) { <div class="alert alert-error">{{ err }}</div> }

    <div class="card"><div class="card-body" style="max-width:640px">
      <form [formGroup]="form" (ngSubmit)="submit()">
        <div class="form-field">
          <label class="label">Name</label>
          <input class="input" formControlName="name" placeholder="corp-lan" />
        </div>
        <div class="form-grid">
          <div class="form-field">
            <label class="label">CIDR</label>
            <input class="input mono" formControlName="cidr" placeholder="10.0.0.0/16" />
            @if (form.controls.cidr.invalid && form.controls.cidr.touched) {
              <span class="field-error">Enter a valid IPv4 CIDR, e.g. 10.0.0.0/24</span>
            }
          </div>
          <div class="form-field">
            <label class="label">Type</label>
            <select class="select" formControlName="type">
              @for (t of types; track t) { <option [value]="t">{{ t }}</option> }
            </select>
          </div>
        </div>
        <div class="form-field">
          <label class="label">Description</label>
          <textarea class="textarea" formControlName="description"></textarea>
        </div>
        <div style="display:flex;gap:10px">
          <button class="btn btn-primary" type="submit" [disabled]="busy()">{{ busy() ? 'Saving…' : 'Save network' }}</button>
          <a class="btn btn-ghost" routerLink="/networks">Cancel</a>
        </div>
      </form>
    </div></div>
  `
})
export class NetworkFormComponent {
  private readonly svc = inject(NetworksService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly types = NETWORK_TYPES;
  readonly busy = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(2)] }),
    cidr: new FormControl('', { nonNullable: true, validators: [Validators.required, cidrValidator()] }),
    type: new FormControl<NetworkType>('Corporate', { nonNullable: true }),
    description: new FormControl('')
  });

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.busy.set(true);
    this.svc.create(this.form.getRawValue()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { this.toast.success('Network created.'); this.router.navigateByUrl('/networks'); },
      error: (err: ApiError) => {
        this.busy.set(false);
        this.error.set(err.fieldErrors.join(' ') || err.detail || err.title);
      }
    });
  }
}