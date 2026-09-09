import { DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, input, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '@core/auth/auth.service';
import { ChangeRequestsService } from '@core/services/change-requests.service';
import { ConfirmService } from '@core/services/confirm.service';
import { ToastService } from '@core/services/toast.service';
import { ApiError, ChangeRequest } from '@core/models';
import { PageHeaderComponent } from '@shared/components/page-header.component';
import { SpinnerComponent } from '@shared/components/spinner.component';
import { StatusBadgeComponent } from '@shared/components/status-badge.component';

@Component({
  selector: 'sq-change-request-detail',
  imports: [DatePipe, PageHeaderComponent, SpinnerComponent, StatusBadgeComponent],
  template: `
    @if (loading()) {
      <div class="page-loading"><sq-spinner large /></div>
    } @else if (cr(); as c) {
      <sq-page-header [title]="c.title" [subtitle]="'Policy: ' + (c.policyName ?? c.policyId)">
        @if (can('submit')) { <button actions class="btn btn-primary" (click)="act('submit')">Submit</button> }
        @if (can('approve')) { <button actions class="btn btn-primary" (click)="act('approve')">Approve</button> }
        @if (can('reject')) { <button actions class="btn btn-danger" (click)="act('reject')">Reject</button> }
        @if (can('deploy')) { <button actions class="btn btn-primary" (click)="act('deploy')">Deploy</button> }
        @if (can('rollback')) { <button actions class="btn btn-danger" (click)="act('rollback')">Rollback</button> }
        @if (can('cancel')) { <button actions class="btn btn-ghost" (click)="act('cancel')">Cancel</button> }
      </sq-page-header>

      @if (actionError(); as err) { <div class="alert alert-error">{{ err }}</div> }

      <div class="grid-2">
        <div class="card">
          <div class="card-head"><span class="card-title">Details</span></div>
          <div class="card-body">
            <div class="kv">
              <span class="kv-key">State</span><span><sq-status-badge [label]="c.state" /></span>
              <span class="kv-key">Risk</span><span><sq-status-badge [label]="c.risk" /></span>
              <span class="kv-key">Requested by</span><span class="kv-val">{{ c.requestedByName ?? c.requestedBy }}</span>
              <span class="kv-key">Created</span><span class="kv-val">{{ c.createdAt | date: 'medium' }}</span>
              <span class="kv-key">Updated</span><span class="kv-val">{{ c.updatedAt | date: 'medium' }}</span>
            </div>
            <h4 style="margin-top:18px">Reason</h4>
            <p class="muted">{{ c.reason }}</p>

            @if (c.approvals.length) {
              <h4>Approvals</h4>
              @for (a of c.approvals; track a.approverId + a.decidedAt) {
                <div class="finding-item">
                  <sq-status-badge [label]="a.decision" />
                  <div>
                    <strong>{{ a.approverName ?? a.approverId }}</strong>
                    <span class="muted small"> · {{ a.decidedAt | date: 'medium' }}</span>
                    @if (a.comment) { <div class="muted small">{{ a.comment }}</div> }
                  </div>
                </div>
              }
            }

            @if (c.deployment; as d) {
              <h4>Deployment</h4>
              <div class="kv">
                <span class="kv-key">Status</span><span><sq-status-badge [label]="d.status" /></span>
                <span class="kv-key">Attempt</span><span class="kv-val">{{ d.attempt ?? 1 }}</span>
                <span class="kv-key">Deployed at</span><span class="kv-val">{{ d.deployedAt | date: 'medium' }}</span>
              </div>
            }
          </div>
        </div>

        <div class="card">
          <div class="card-head"><span class="card-title">Workflow timeline</span></div>
          <div class="card-body">
            <div class="timeline">
              @for (entry of timeline(c); track entry.at + entry.state; let i = $index) {
                <div class="timeline-item">
                  <div class="timeline-dot"></div>
                  <strong><sq-status-badge [label]="entry.state" /></strong>
                  <div class="timeline-when">{{ entry.at | date: 'medium' }} @if (entry.by) { · {{ entry.by }} }</div>
                  @if (entry.note) { <div class="muted small">{{ entry.note }}</div> }
                </div>
              }
            </div>
          </div>
        </div>
      </div>
    }
  `
})
export class ChangeRequestDetailComponent implements OnInit {
  readonly id = input.required<string>();

  private readonly svc = inject(ChangeRequestsService);
  private readonly auth = inject(AuthService);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly cr = signal<ChangeRequest | null>(null);
  readonly actionError = signal<string | null>(null);

  ngOnInit(): void { this.load(); }

  timeline(c: ChangeRequest) {
    return c.timeline?.length
      ? c.timeline
      : [{ state: c.state, at: c.updatedAt, by: c.requestedByName }];
  }

  can(action: 'submit' | 'approve' | 'reject' | 'deploy' | 'rollback' | 'cancel'): boolean {
    const c = this.cr();
    if (!c) return false;
    const eng = this.auth.hasRole('Admin', 'SecurityEngineer');
    switch (action) {
      case 'submit': return c.state === 'Draft' && eng;
      case 'approve':
      case 'reject': return c.state === 'AwaitingApproval' && this.auth.hasRole('Admin', 'Approver');
      case 'deploy': return c.state === 'Approved' && eng;
      case 'rollback': return (c.state === 'Verification' || c.state === 'Failed') && eng;
      case 'cancel': return ['Draft', 'Submitted'].includes(c.state) && eng;
    }
  }

  async act(action: 'submit' | 'approve' | 'reject' | 'deploy' | 'rollback' | 'cancel'): Promise<void> {
    const c = this.cr();
    if (!c) return;

    if (['deploy', 'rollback', 'reject'].includes(action)) {
      const ok = await this.confirm.confirm({
        title: `Confirm ${action}`,
        message: `Are you sure you want to ${action} change request "${c.title}"?`,
        confirmLabel: action[0].toUpperCase() + action.slice(1),
        danger: action !== 'deploy'
      });
      if (!ok) return;
    }

    this.actionError.set(null);
    const comment = action === 'reject' ? window.prompt('Rejection reason (audited):') ?? undefined : undefined;

    const request$ = {
      submit: this.svc.submit(c.id),
      approve: this.svc.approve(c.id),
      reject: this.svc.reject(c.id, comment),
      deploy: this.svc.deploy(c.id),
      rollback: this.svc.rollback(c.id),
      cancel: this.svc.cancel(c.id)
    }[action];

    request$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: updated => {
        this.cr.set(updated);
        this.toast.success(`Change request ${action} succeeded. New state: ${updated.state}`);
      },
      error: (err: ApiError) => this.actionError.set(err.detail ?? err.title)
    });
  }

  private load(): void {
    this.svc.get(this.id()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: c => { this.cr.set(c); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }
}