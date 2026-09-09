import { DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, input, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { TestRunsService } from '@core/services/test-runs.service';
import { ToastService } from '@core/services/toast.service';
import { isTerminalRunStatus, TestResult, TestRun } from '@core/models';
import { DataTableComponent, TableColumn } from '@shared/components/data-table.component';
import { PageHeaderComponent } from '@shared/components/page-header.component';
import { SpinnerComponent } from '@shared/components/spinner.component';
import { StatusBadgeComponent } from '@shared/components/status-badge.component';

@Component({
  selector: 'sq-test-run-detail',
  imports: [RouterLink, DatePipe, PageHeaderComponent, SpinnerComponent, StatusBadgeComponent, DataTableComponent],
  template: `
    @if (loading()) {
      <div class="page-loading"><sq-spinner large /></div>
    } @else if (run(); as r) {
      <sq-page-header [title]="'Test Run ' + r.id.slice(0, 8)"
                      [subtitle]="(r.testSuiteName ?? r.testSuiteId) + ' · ' + r.environment">
        @if (!terminal(r.status)) {
          <button actions class="btn btn-danger" (click)="cancel()">Cancel run</button>
        }
        <span actions class="muted small" style="align-self:center">
          @if (!terminal(r.status)) { auto-refreshing every 3s… }
        </span>
      </sq-page-header>

      <div class="card"><div class="card-body">
        <div style="display:flex;align-items:center;gap:16px;margin-bottom:12px">
          <sq-status-badge [label]="r.status" />
          <strong>{{ r.passed }} passed</strong>
          <span class="muted">{{ r.failed }} failed · {{ r.skipped }} skipped · {{ r.totalCases }} total</span>
          @if (r.startedAt) { <span class="muted small">started {{ r.startedAt | date: 'HH:mm:ss' }}</span> }
        </div>
        <div class="progress">
          <div class="progress-bar progress-pass" [style.width.%]="pct(r.passed, r.totalCases)"></div>
          <div class="progress-bar progress-fail" [style.width.%]="pct(r.failed, r.totalCases)"></div>
          <div class="progress-bar progress-skip" [style.width.%]="pct(r.skipped, r.totalCases)"></div>
        </div>
      </div></div>

      <div class="card">
        <div class="card-head"><span class="card-title">Results</span></div>
        <sq-data-table [columns]="columns" [rows]="results()" emptyIcon="⏳" emptyTitle="Waiting for results…" />
      </div>
    }
  `
})
export class TestRunDetailComponent implements OnInit {
  readonly id = input.required<string>();

  private readonly svc = inject(TestRunsService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly run = signal<TestRun | null>(null);
  readonly results = signal<TestResult[]>([]);
  private timer?: ReturnType<typeof setTimeout>;

  readonly columns: TableColumn<TestResult>[] = [
    { key: 'testCaseName', header: 'Test Case', value: x => x.testCaseName ?? x.testCaseId.slice(0, 8) },
    { key: 'status', header: 'Status', type: 'badge' },
    { key: 'durationMs', header: 'Duration', align: 'right', value: x => (x.durationMs != null ? `${x.durationMs} ms` : '—') },
    { key: 'traceId', header: 'Trace ID', type: 'mono', value: x => x.traceId?.slice(0, 16) ?? '—' },
    { key: 'failureMessage', header: 'Failure', value: x => x.failureMessage ?? '—' },
    {
      key: '_defect', header: '', value: x =>
        x.status === 'Failed' ? '🐞 report' : ''
    }
  ];

  ngOnInit(): void { this.refresh(); }

  terminal(status: string): boolean {
    return isTerminalRunStatus(status as TestRun['status']);
  }

  pct(part: number, total: number): number {
    return total === 0 ? 0 : (part / total) * 100;
  }

  cancel(): void {
    this.svc.cancel(this.id()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: run => { this.run.set(run); this.toast.info('Test run cancellation requested.'); },
      error: () => this.toast.error('Cancel failed.')
    });
  }

  private refresh(): void {
    const id = this.id();
    this.svc.get(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: run => {
        this.run.set(run);
        this.loading.set(false);
        this.svc.results(id).pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({ next: res => this.results.set(res) });

        if (!isTerminalRunStatus(run.status)) {
          this.timer = setTimeout(() => this.refresh(), 3000);
          this.destroyRef.onDestroy(() => clearTimeout(this.timer));
        }
      },
      error: () => this.loading.set(false)
    });
  }
}