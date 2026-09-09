import { Component, DestroyRef, inject, input, OnInit, signal } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FirewallsService } from '@core/services/firewalls.service';
import { PoliciesService } from '@core/services/policies.service';
import { ToastService } from '@core/services/toast.service';
import {
  ApiError, DIRECTIONS, ENVIRONMENTS, Firewall, PROTOCOLS, Protocol,
  PolicyRuleDraft, RuleAction, RULE_ACTIONS
} from '@core/models';
import { PageHeaderComponent } from '@shared/components/page-header.component';
import { cidrValidator, portValidator } from '@shared/validators/security.validators';

@Component({
  selector: 'sq-policy-form',
  imports: [ReactiveFormsModule, RouterLink, PageHeaderComponent],
  template: `
    <sq-page-header [title]="id() ? 'Edit Policy' : 'Create Policy'"
                    subtitle="Security policy changes are software changes — validate before deploy" />

    @if (error(); as err) { <div class="alert alert-error">{{ err }}</div> }

    <form [formGroup]="form" (ngSubmit)="submit()">
      <div class="card"><div class="card-body">
        <div class="form-grid">
          <div class="form-field">
            <label class="label">Policy name</label>
            <input class="input" formControlName="name" placeholder="edge-ingress-rules" />
          </div>
          <div class="form-field">
            <label class="label">Firewall</label>
            <select class="select" formControlName="firewallId">
              <option value="" disabled>Select firewall…</option>
              @for (fw of firewalls(); track fw.id) { <option [value]="fw.id">{{ fw.name }} ({{ fw.environment }})</option> }
            </select>
          </div>
          <div class="form-field">
            <label class="label">Environment</label>
            <select class="select" formControlName="environment">
              @for (e of environments; track e) { <option [value]="e">{{ e }}</option> }
            </select>
          </div>
        </div>
        <div class="form-field">
          <label class="label">Description</label>
          <textarea class="textarea" formControlName="description"></textarea>
        </div>
      </div></div>

      <div class="card">
        <div class="card-head">
          <span class="card-title">Rules ({{ rules.length }})</span>
          <button type="button" class="btn btn-sm" (click)="addRule()">+ Add rule</button>
        </div>
        <div class="card-body" formArrayName="rules">
          @for (rule of rules.controls; track $index; let i = $index) {
            <div class="rule-grid" [formGroupName]="i">
              <div class="form-field"><label class="label">Priority</label>
                <input class="input" type="number" formControlName="priority" /></div>
              <div class="form-field"><label class="label">Source CIDR</label>
                <input class="input mono" formControlName="sourceCidr" placeholder="10.0.0.0/16" />
                @if (rule.get('sourceCidr')?.invalid) { <span class="field-error">Invalid CIDR</span> }</div>
              <div class="form-field"><label class="label">Destination CIDR</label>
                <input class="input mono" formControlName="destinationCidr" placeholder="192.168.1.0/24" />
                @if (rule.get('destinationCidr')?.invalid) { <span class="field-error">Invalid CIDR</span> }</div>
              <div class="form-field"><label class="label">Protocol</label>
                <select class="select" formControlName="protocol">
                  @for (p of protocols; track p) { <option [value]="p">{{ p }}</option> }
                </select></div>
              <div class="form-field"><label class="label">Port</label>
                <input class="input" type="number" formControlName="port" />
                @if (rule.get('port')?.invalid) { <span class="field-error">0–65535</span> }</div>
              <div class="form-field"><label class="label">Action</label>
                <select class="select" formControlName="action">
                  @for (a of actions; track a) { <option [value]="a">{{ a }}</option> }
                </select></div>
              <div class="form-field"><label class="label">Direction</label>
                <select class="select" formControlName="direction">
                  @for (d of directions; track d) { <option [value]="d">{{ d }}</option> }
                </select></div>
              <div class="form-field"><label class="label">Log</label>
                <label class="checkbox-row" style="margin:8px 0 0"><input type="checkbox" formControlName="loggingEnabled" />on</label></div>
              <div class="form-field"><label class="label">Description</label>
                <input class="input" formControlName="description" /></div>
              <button type="button" class="btn btn-ghost btn-sm" title="Remove rule" (click)="removeRule(i)">✕</button>
            </div>
          }
        </div>
      </div>

      <div style="display:flex;gap:10px">
        <button class="btn btn-primary" type="submit" [disabled]="busy() || form.invalid">
          {{ busy() ? 'Saving…' : (id() ? 'Save policy' : 'Create policy') }}
        </button>
        <a class="btn btn-ghost" routerLink="/policies">Cancel</a>
      </div>
    </form>
  `
})
export class PolicyFormComponent implements OnInit {
  readonly id = input<string>();

  private readonly fb = inject(FormBuilder);
  private readonly svc = inject(PoliciesService);
  private readonly firewallsSvc = inject(FirewallsService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly environments = ENVIRONMENTS;
  readonly protocols = PROTOCOLS;
  readonly actions = RULE_ACTIONS;
  readonly directions = DIRECTIONS;

  readonly firewalls = signal<Firewall[]>([]);
  readonly busy = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    firewallId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    name: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(3)] }),
    environment: new FormControl('QA', { nonNullable: true, validators: [Validators.required] }),
    description: [''],
    rules: this.fb.nonNullable.array([this.createRule()])
  });

  get rules(): FormArray { return this.form.controls.rules; }

  ngOnInit(): void {
    this.firewallsSvc.list({ pageSize: 100 }).pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: r => this.firewalls.set(r.items) });

    const id = this.id();
    if (!id) return;
    this.svc.get(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: policy => {
        this.form.patchValue({
          firewallId: policy.firewallId,
          name: policy.name,
          environment: policy.environment,
          description: policy.description ?? ''
        });
        this.rules.clear();
        for (const rule of policy.rules) {
          this.rules.push(this.createRule(rule));
        }
      },
      error: () => this.error.set('Policy not found.')
    });
  }

  createRule(partial?: Partial<PolicyRuleDraft>): FormGroup {
    return this.fb.nonNullable.group({
      priority: [partial?.priority ?? (this.rules?.length ?? 0) * 10 + 10, [Validators.required, Validators.min(1)]],
      sourceCidr: [partial?.sourceCidr ?? '', [Validators.required, cidrValidator()]],
      destinationCidr: [partial?.destinationCidr ?? '', [Validators.required, cidrValidator()]],
      protocol: [partial?.protocol ?? ('TCP' as Protocol), Validators.required],
      port: [partial?.port ?? 443, [Validators.required, portValidator()]],
      action: [partial?.action ?? ('Allow' as RuleAction), Validators.required],
      direction: [partial?.direction ?? 'Inbound'],
      loggingEnabled: [partial?.loggingEnabled ?? true],
      description: [partial?.description ?? '']
    });
  }

  addRule(): void { this.rules.push(this.createRule()); }
  removeRule(index: number): void { this.rules.removeAt(index); }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.busy.set(true);
    this.error.set(null);

    const value = this.form.getRawValue();
    const payload = {
      firewallId: value.firewallId,
      name: value.name,
      environment: value.environment,
      description: value.description,
      rules: value.rules
    };

    const request$ = this.id()
      ? this.svc.update(this.id()!, { ...payload, version: 0 })
      : this.svc.create(payload);

    request$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: policy => {
        this.toast.success(`Policy "${policy.name}" saved.`);
        this.router.navigateByUrl(`/policies/${policy.id}`);
      },
      error: (err: ApiError) => {
        this.busy.set(false);
        this.error.set(
          err.status === 409
            ? 'Conflict: the policy was modified by someone else. Reload and retry.'
            : err.fieldErrors.join(' ') || err.detail || err.title
        );
      }
    });
  }
}