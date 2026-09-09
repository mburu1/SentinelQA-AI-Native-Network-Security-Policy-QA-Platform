import { DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, input, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { PoliciesService } from '@core/services/policies.service';
import { ToastService } from '@core/services/toast.service';
import { Policy, PolicyAnalysis, PolicyValidationResult } from '@core/models';
import { HasRoleDirective } from '@shared/directives/has-role.directive';
import { DataTableComponent, TableColumn } from '@shared/components/data-table.component';
import { PageHeaderComponent } from '@shared/components/page-header.component';
import { SpinnerComponent } from '@shared/components/spinner.component';
import { StatusBadgeComponent } from '@shared/components/status-badge.component';
import { PolicyRule } from '@core/models';

@Component({
  selector: 'sq-policy-detail',
  imports: [RouterLink, DatePipe, HasRoleDirective, PageHeaderComponent, SpinnerComponent,
            StatusBadgeComponent, DataTableComponent],
  template: `
    @if (loading()) {
      <div class="page-loading"><sq-spinner large /></div>
    } @else if (policy(); as p) {
      <sq-page-header [title]="p.name" [subtitle]="(p.description ?? '') + ' · v' + p.version + ' · ' + p.environment">
        <button actions class="btn" (click)="validate()" [disabled]="validating()"
                *sqHasRole="['Admin','SecurityEngineer','QAEngineer']" data-testid="validate-policy">
          {{ validating() ? 'Analyzing…' : '⚡ Validate' }}
        </button>
        <a actions class="btn" [routerLink]="'/policies/' + p.id + '/edit'"
           *sqHasRole="['Admin','SecurityEngineer','Developer']">Edit</a>
        <a actions class="btn btn-primary" routerLink="/change-requests/new" [queryParams]="{ policyId: p.id }"
           *sqHasRole="['Admin','SecurityEngineer']">Submit change request</a>
      </sq-page-header>

      <div class="grid-2">
        <div class="card">
          <div class="card-head"><span class="card-title">Rules ({{ p.rules?.length ?? 0 }})</span>
            <span class="muted small">Ordered by priority — first match wins</span></div>
          <sq-data-table [columns]="ruleColumns" [rows]="p.rules ?? []"
                         emptyIcon="📜" emptyTitle="No rules defined" />
        </div>

        <div class="card">
          <div class="card-head"><span class="card-title">Analysis</span>
            @if (analysis(); as a) { <span class="muted small">{{ a.analyzedAt | date: 'medium' }}</span> }</div>
          <div class="card-body">
            @if (validating()) {
              <div class="page-loading"><sq-spinner /></div>
            } @else if (validation(); as v) {
              @if (v.isValid) {
                <div class="alert alert-success">✅ Policy passed all engine checks.</div>
              } @else {
                <div class="alert alert-error">⛔ {{ v.findings.length }} finding(s) detected by the policy engine.</div>
              }
              @for (f of v.findings; track f.code + (f.rulePriority ?? 0)) {
                <div class="finding-item">
                  <sq-status-badge [label]="f.severity" />
                  <div>
                    <strong>{{ f.title }}</strong>
                    @if (f.rulePriority) { <span class="chip" style="margin-left:8px">rule #{{ f.rulePriority }}</span> }
                    <div class="muted small">{{ f.message }}</div>
                    @if (f.recommendation) { <div class="small" style="margin-top:4px">💡 {{ f.recommendation }}</div> }
                  </div>
                </div>
              }
            } @else if (analysis(); as a) {
              <div class="kv">
                <span class="kv-key">Total rules</span><span class="kv-val">{{ a.statistics.totalRules }}</span>
                <span class="kv-key">Allow / Deny</span><span class="kv-val">{{ a.statistics.allowRules }} / {{ a.statistics.denyRules }}</span>
                <span class="kv-key">Shadowed</span><span class="kv-val">{{ a.statistics.shadowedRules }}</span>
                <span class="kv-key">Conflicting</span><span class="kv-val">{{ a.statistics.conflictingRules }}</span>
                <span class="kv-key">Duplicates</span><span class="kv-val">{{ a.statistics.duplicateRules }}</span>
              </div>
              @for (f of a.findings; track f.code) {
                <div class="finding-item">
                  <sq-status-badge [label]="f.severity" />
                  <div><strong>{{ f.title }}</strong><div class="muted small">{{ f.message }}</div></div>
                </div>
              }
            } @else {
              <div class="muted">Run <strong>Validate</strong> to execute the network security rules engine
              (CIDR checks, duplicates, overlaps, conflicts, shadowing, public exposure…).</div>
            }
          </div>
        </div>
      </div>
    }
  `
})
export class PolicyDetailComponent implements OnInit {
  readonly id = input.required<string>();

  private readonly svc = inject(PoliciesService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly validating = signal(false);
  readonly policy = signal<Policy | null>(null);
  readonly analysis = signal<PolicyAnalysis | null>(null);
  readonly validation = signal<PolicyValidationResult | null>(null);

  readonly ruleColumns: TableColumn<PolicyRule>[] = [
    { key: 'priority', header: '#', align: 'right' },
    { key: 'action', header: 'Action', type: 'badge' },
    { key: 'sourceCidr', header: 'Source', type: 'mono' },
    { key: 'destinationCidr', header: 'Destination', type: 'mono' },
    { key: 'protocol', header: 'Protocol', value: r => `${r.protocol}/${r.port || '*'}`, type: 'text' },
    { key: 'direction', header: 'Direction' },
    { key: 'loggingEnabled', header: 'Log', value: r => (r.loggingEnabled ? '✓' : '—') },
    { key: 'description', header: 'Description' }
  ];

  ngOnInit(): void {
    const id = this.id();
    this.svc.get(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: p => { this.policy.set(p); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
    this.svc.analysis(id).pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: a => this.analysis.set(a), error: () => undefined });
  }

  validate(): void {
    this.validating.set(true);
    this.svc.validate(this.id()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: v => {
        this.validation.set(v);
        this.validating.set(false);
        this.toast[v.isValid ? 'success' : 'warning'](
          v.isValid ? 'Policy validation passed.' : `Validation found ${v.findings.length} issue(s).`
        );
      },
      error: () => this.validating.set(false)
    });
  }
}