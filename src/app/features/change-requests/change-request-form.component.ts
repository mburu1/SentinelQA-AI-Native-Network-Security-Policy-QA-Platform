import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ChangeRequestsService } from '@core/services/change-requests.service';
import { PoliciesService } from '@core/services/policies.service';
import { ToastService } from '@core/services/toast.service';
import { ApiError, CHANGE_RISKS, ChangeRisk, Policy } from '@core/models';
import { PageHeaderComponent } from '@shared/components/page-header.component';

@Component({
  selector: 'sq-change-request-form',
  imports: [ReactiveFormsModule, RouterLink, PageHeaderComponent],
  template: `
    <sq-page-header title="New Change Request" subtitle="Policy changes must be reviewed, tested and approved before deployment" />

    @if (error(); as err) { <div class="alert alert-error">{{ err }}</div> }

    <div class="card"><div class="card-body" style="max-width:680px">
      <form [formGroup]="form" (ngSubmit)="submit()">
        <div class="form-field">
          <label class="label">Policy</label>
          <select class="select" formControlName="policyId">
            <option value="" disabled>Select policy…</option>
            @for (p of policies(); track p.id) { <option [value]="p.id">{{ p.name }} (v{{ p.version }})</option> }
          </select>
        </div>
        <div class="form-field">
          <label class="label">Title</label>
          <input class="input" formControlName="title" placeholder="Open TCP/8443 for payments service" />
        </div>
        <div class="form-field">
          <label class="label">Business reason / justification</label>
          <textarea class="textarea" formControlName="reason"></textarea>
          @if (form.controls.reason.invalid && form.controls.reason.touched) {
            <span class="field-error">Justification is required (min 10 characters) — audit requirement.</span>
          }
        </div>
        <div class="form-field" style="max-width:220px">
          <label class="label">Risk</label>
          <select class="select" formControlName="risk">
            @for (r of risks; track r) { <option [value]="r">{{ r }}</option> }
          </select>
        </div>
        <div style="display:flex;gap:10px">
          <button class="btn btn-primary" type="submit" [disabled]="busy()">{{ busy() ? 'Creating…' : 'Create draft' }}</button>
          <a class="btn btn-ghost" routerLink="/change-requests">Cancel</a>
        </div>
      </form>
    </div></div>
  `
})
export class ChangeRequestFormComponent implements OnInit {
  private readonly svc = inject(ChangeRequestsService);
  private readonly policiesSvc = inject(PoliciesService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly risks = CHANGE_RISKS;
  readonly policies = signal<Policy[]>([]);
  readonly busy = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = new FormGroup({
    policyId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    title: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(5)] }),
    reason: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(10)] }),
    risk: new FormControl<ChangeRisk>('Medium', { nonNullable: true })
  });

  ngOnInit(): void {
    this.policiesSvc.list({ pageSize: 100 }).pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: r => this.policies.set(r.items) });

    const policyId = this.route.snapshot.queryParamMap.get('policyId');
    if (policyId) this.form.controls.policyId.setValue(policyId);
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.busy.set(true);
    this.svc.create(this.form.getRawValue()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: cr => {
        this.toast.success('Change request created as Draft.');
        this.router.navigateByUrl(`/change-requests/${cr.id}`);
      },
      error: (err: ApiError) => {
        this.busy.set(false);
        this.error.set(err.fieldErrors.join(' ') || err.detail || err.title);
      }
    });
  }
}