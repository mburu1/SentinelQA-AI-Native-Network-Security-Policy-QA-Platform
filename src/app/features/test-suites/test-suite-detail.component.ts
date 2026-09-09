import { Component, DestroyRef, inject, input, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { TestSuitesService } from '@core/services/test-suites.service';
import { TestRunsService } from '@core/services/test-runs.service';
import { ToastService } from '@core/services/toast.service';
import {
  ENVIRONMENTS, TestCase, TEST_CASE_PRIORITIES, TestCasePriority,
  TEST_CASE_TYPES, TestCaseType, TestSuite
} from '@core/models';
import { DataTableComponent, TableColumn } from '@shared/components/data-table.component';
import { PageHeaderComponent } from '@shared/components/page-header.component';
import { SpinnerComponent } from '@shared/components/spinner.component';
import { StatusBadgeComponent } from '@shared/components/status-badge.component';

@Component({
  selector: 'sq-test-suite-detail',
  imports: [PageHeaderComponent, SpinnerComponent, StatusBadgeComponent, DataTableComponent],
  template: `
    @if (loading()) {
      <div class="page-loading"><sq-spinner large /></div>
    } @else if (suite(); as s) {
      <sq-page-header [title]="s.name" [subtitle]="(s.description ?? '') + (s.isRegression ? ' · Regression pack' : '')">
        <select actions class="select" style="width:160px" [value]="runEnv()"
                (change)="runEnv.set($any($event.target).value)">
          @for (e of environments; track e) { <option [value]="e">{{ e }}</option> }
        </select>
        <button actions class="btn btn-primary" (click)="run()" data-testid="run-suite">▶ Run suite</button>
      </sq-page-header>

      <div class="card">
        <div class="card-head"><span class="card-title">Test cases ({{ cases().length }})</span></div>
        <sq-data-table [columns]="columns" [rows]="cases()" emptyIcon="🧪" emptyTitle="No test cases yet"
                       emptyMessage="Add cases manually or generate them with the AI QA Copilot." />
      </div>

      <div class="card">
        <div class="card-head"><span class="card-title">Add test case</span></div>
        <div class="card-body">
          <div class="form-grid">
            <div class="form-field"><label class="label">Name</label>
              <input class="input" [value]="draft().name" (input)="patch('name', $any($event.target).value)" /></div>
            <div class="form-field"><label class="label">Type</label>
              <select class="select" [value]="draft().type" (change)="patch('type', $any($event.target).value)">
                @for (t of types; track t) { <option [value]="t">{{ t }}</option> }
              </select></div>
            <div class="form-field"><label class="label">Priority</label>
              <select class="select" [value]="draft().priority" (change)="patch('priority', $any($event.target).value)">
                @for (p of priorities; track p) { <option [value]="p">{{ p }}</option> }
              </select></div>
          </div>
          <div class="form-field"><label class="label">Expected result</label>
            <input class="input" [value]="draft().expected" (input)="patch('expected', $any($event.target).value)" /></div>
          <button class="btn btn-primary" (click)="addCase()">Add case</button>
        </div>
      </div>
    }
  `
})
export class TestSuiteDetailComponent implements OnInit {
  readonly id = input.required<string>();

  private readonly svc = inject(TestSuitesService);
  private readonly runs = inject(TestRunsService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly environments = ENVIRONMENTS;
  readonly types = TEST_CASE_TYPES;
  readonly priorities = TEST_CASE_PRIORITIES;

  readonly loading = signal(true);
  readonly suite = signal<TestSuite | null>(null);
  readonly cases = signal<TestCase[]>([]);
  readonly runEnv = signal('QA');
  readonly draft = signal({ name: '', type: 'Positive' as TestCaseType, priority: 'Medium' as TestCasePriority, expected: '' });

  readonly columns: TableColumn<TestCase>[] = [
    { key: 'name', header: 'Test Case' },
    { key: 'type', header: 'Type', type: 'badge' },
    { key: 'priority', header: 'Priority', type: 'badge' },
    { key: 'automated', header: 'Automated', value: c => (c.automated ? '✓' : 'manual') },
    { key: 'expected', header: 'Expected', value: c => c.expected ?? '—' }
  ];

  ngOnInit(): void {
    const id = this.id();
    this.svc.get(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: s => { this.suite.set(s); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
    this.loadCases();
  }

  patch<K extends keyof ReturnType<typeof this.draft>>(key: K, value: string): void {
    this.draft.update(d => ({ ...d, [key]: value } as typeof d));
  }

  addCase(): void {
    const d = this.draft();
    if (!d.name.trim()) return;
    this.svc.addCase({
      testSuiteId: this.id(), name: d.name, type: d.type, priority: d.priority, expected: d.expected
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.toast.success('Test case added.');
        this.draft.set({ name: '', type: 'Positive', priority: 'Medium', expected: '' });
        this.loadCases();
      },
      error: () => this.toast.error('Could not add test case.')
    });
  }

  run(): void {
    this.runs.start({ testSuiteId: this.id(), environment: this.runEnv() })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: run => {
          this.toast.info(`Test run ${run.id.slice(0, 8)} accepted (202).`);
          this.router.navigateByUrl(`/test-runs/${run.id}`);
        },
        error: () => this.toast.error('Could not start test run.')
      });
  }

  private loadCases(): void {
    this.svc.listCases(this.id()).pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: c => this.cases.set(c), error: () => undefined });
  }
}