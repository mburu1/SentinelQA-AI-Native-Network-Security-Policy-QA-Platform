import { DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, input, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DefectsService } from '@core/services/defects.service';
import { ToastService } from '@core/services/toast.service';
import { ApiError, Defect, DEFECT_TRANSITIONS, DefectStatus } from '@core/models';
import { PageHeaderComponent } from '@shared/components/page-header.component';
import { SpinnerComponent } from '@shared/components/spinner.component';
import { StatusBadgeComponent } from '@shared/components/status-badge.component';

@Component({
  selector: 'sq-defect-detail',
  imports: [DatePipe, PageHeaderComponent, SpinnerComponent, StatusBadgeComponent],
  template: `
    @if (loading()) {
      <div class="page-loading"><sq-spinner large /></div>
    } @else if (defect(); as d) {
      <sq-page-header [title]="d.title" [subtitle]="d.component ?? ''">
        @for (next of transitions(d.status); track next) {
          <button actions class="btn" [class.btn-primary]="next !== 'InProgress'" (click)="transition(next)">
            → {{ next }}
          </button>
        }
      </sq-page-header>

      @if (error(); as err) { <div class="alert alert-error">{{ err }}</div> }

      <div class="grid-2">
        <div class="card">
          <div class="card-head"><span class="card-title">Details</span></div>
          <div class="card-body">
            <div class="kv">
              <span class="kv-key">Status</span><span><sq-status-badge [label]="d.status" /></span>
              <span class="kv-key">Severity</span><span><sq-status-badge [label]="d.severity" /></span>
              <span class="kv-key">Priority</span><span><sq-status-badge [label]="d.priority" /></span>
              <span class="kv-key">Environment</span><span class="kv-val">{{ d.environment }}</span>
              <span class="kv-key">Assignee</span><span class="kv-val">{{ d.assigneeName ?? 'Unassigned' }}</span>
              <span class="kv-key">Reported by</span><span class="kv-val">{{ d.reportedBy ?? '—' }}</span>
              <span class="kv-key">Created</span><span class="kv-val">{{ d.createdAt | date: 'medium' }}</span>
              <span class="kv-key">Trace ID</span><span class="kv-val mono">{{ d.traceId ?? '—' }}</span>
              <span class="kv-key">Test run</span><span class="kv-val mono">{{ d.testRunId ?? '—' }}</span>
            </div>
          </div>
        </div>

        <div class="card">
          <div class="card-head"><span class="card-title">Reproduction</span></div>
          <div class="card-body">
            <h4>Description</h4><p class="muted">{{ d.description }}</p>
            @if (d.stepsToReproduce) { <h4>Steps</h4><pre class="code-block">{{ d.stepsToReproduce }}</pre> }
            <div class="grid-2">
              <div><h4>Expected</h4><p class="muted">{{ d.expectedResult ?? '—' }}</p></div>
              <div><h4>Actual</h4><p class="muted">{{ d.actualResult ?? '—' }}</p></div>
            </div>
          </div>
        </div>
      </div>
    }
  `
})
export class DefectDetailComponent implements OnInit {
  readonly id = input.required<string>();

  private readonly svc = inject(DefectsService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly defect = signal<Defect | null>(null);
  readonly error = signal<string | null>(null);

  ngOnInit(): void { this.load(); }

  transitions(status: DefectStatus): DefectStatus[] {
    return DEFECT_TRANSITIONS[status] ?? [];
  }

  transition(status: DefectStatus): void {
    this.error.set(null);
    const comment = status === 'Retest' ? window.prompt('Retest result note (Passed/Failed):') ?? undefined : undefined;
    this.svc.transition(this.id(), status, comment).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: d => { this.defect.set(d); this.toast.success(`Defect moved to ${d.status}.`); },
      error: (err: ApiError) => this.error.set(err.detail ?? err.title)
    });
  }

  private load(): void {
    this.svc.get(this.id()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: d => { this.defect.set(d); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }
}