import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { DashboardService, DashboardSummary } from '@core/services/dashboard.service';
import { DataTableComponent, TableColumn } from '@shared/components/data-table.component';
import { PageHeaderComponent } from '@shared/components/page-header.component';
import { SpinnerComponent } from '@shared/components/spinner.component';
import { StatusBadgeComponent } from '@shared/components/status-badge.component';
import { ChangeRequest, Defect, TestRun } from '@core/models';
import { TimeAgoPipe } from '@shared/pipes/common.pipes';

@Component({
  selector: 'sq-dashboard',
  imports: [RouterLink, DatePipe, TimeAgoPipe, PageHeaderComponent, SpinnerComponent,
            StatusBadgeComponent, DataTableComponent],
  template: `
    <sq-page-header title="Dashboard" subtitle="Security posture and quality engineering at a glance" />

    @if (loading()) {
      <div class="page-loading"><sq-spinner large /></div>
    } @else if (summary(); as s) {
      <div class="stat-grid">
        <div class="stat-card"><div><div class="stat-value">{{ s.firewalls }}</div><div class="stat-label">Firewalls</div></div><div class="stat-icon">🧱</div></div>
        <div class="stat-card"><div><div class="stat-value">{{ s.policies }}</div><div class="stat-label">Policies</div></div><div class="stat-icon">📜</div></div>
        <div class="stat-card"><div><div class="stat-value">{{ s.openChangeRequests }}</div><div class="stat-label">Open Changes</div></div><div class="stat-icon">🔁</div></div>
        <div class="stat-card"><div><div class="stat-value">{{ s.openDefects }}</div><div class="stat-label">Open Defects</div></div><div class="stat-icon">🐞</div></div>
        <div class="stat-card"><div><div class="stat-value">{{ s.criticalDefects }}</div><div class="stat-label">Critical Defects</div></div><div class="stat-icon">🚨</div></div>
      </div>

      <div class="grid-2">
        <div class="card">
          <div class="card-head"><span class="card-title">Recent Change Requests</span>
            <a class="btn btn-ghost btn-sm" routerLink="/change-requests">View all</a></div>
          <sq-data-table [columns]="crColumns" [rows]="s.recentChangeRequests" clickable
                         (rowClick)="goTo('/change-requests/' + $event.id)"
                         emptyIcon="🔁" emptyTitle="No change requests yet" />
        </div>
        <div class="card">
          <div class="card-head"><span class="card-title">Recent Test Runs</span>
            <a class="btn btn-ghost btn-sm" routerLink="/test-runs">View all</a></div>
          <sq-data-table [columns]="runColumns" [rows]="s.recentTestRuns" clickable
                         (rowClick)="goTo('/test-runs/' + $event.id)"
                         emptyIcon="▶️" emptyTitle="No test runs yet" />
        </div>
      </div>

      <div class="card">
        <div class="card-head"><span class="card-title">Recent Defects</span>
          <a class="btn btn-ghost btn-sm" routerLink="/defects">View all</a></div>
        <sq-data-table [columns]="defectColumns" [rows]="s.recentDefects" clickable
                       (rowClick)="goTo('/defects/' + $event.id)"
                       emptyIcon="🐞" emptyTitle="No defects reported" />
      </div>
    }
  `
})
export class DashboardComponent implements OnInit {
  private readonly dashboard = inject(DashboardService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly summary = signal<DashboardSummary | null>(null);

  readonly crColumns: TableColumn<ChangeRequest>[] = [
    { key: 'title', header: 'Title' },
    { key: 'state', header: 'State', type: 'badge' },
    { key: 'risk', header: 'Risk', type: 'badge' },
    { key: 'updatedAt', header: 'Updated', value: r => r.updatedAt, type: 'text' }
  ];

  readonly runColumns: TableColumn<TestRun>[] = [
    { key: 'testSuiteName', header: 'Suite', value: r => r.testSuiteName ?? r.testSuiteId.slice(0, 8) },
    { key: 'status', header: 'Status', type: 'badge' },
    { key: 'passed', header: 'Passed', align: 'right', value: r => `${r.passed}/${r.totalCases}` },
    { key: 'startedAt', header: 'Started', type: 'datetime' }
  ];

  readonly defectColumns: TableColumn<Defect>[] = [
    { key: 'title', header: 'Title' },
    { key: 'severity', header: 'Severity', type: 'badge' },
    { key: 'status', header: 'Status', type: 'badge' },
    { key: 'createdAt', header: 'Created', type: 'date' }
  ];

  private readonly currency = CurrencyPipe;

  ngOnInit(): void {
    this.dashboard.summary().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: s => { this.summary.set(s); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  goTo(url: string): void {
    location.assign(url);
  }
}