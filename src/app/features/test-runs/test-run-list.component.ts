import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { TestRunsService } from '@core/services/test-runs.service';
import { emptyPage, PagedResult, TestRun } from '@core/models';
import { DataTableComponent, TableColumn } from '@shared/components/data-table.component';
import { PageHeaderComponent } from '@shared/components/page-header.component';
import { PaginationComponent } from '@shared/components/pagination.component';

@Component({
  selector: 'sq-test-run-list',
  imports: [PageHeaderComponent, DataTableComponent, PaginationComponent],
  template: `
    <sq-page-header title="Test Runs" subtitle="Automated executions dispatched through RabbitMQ workers" />

    <div class="card">
      <sq-data-table [columns]="columns" [rows]="result().items" [loading]="loading()" clickable
                     (rowClick)="open($event)" emptyIcon="▶️" emptyTitle="No test runs yet" />
      <sq-pagination [page]="page()" [totalPages]="result().totalPages" [total]="result().totalCount"
                     (pageChange)="page.set($event); load()" />
    </div>
  `
})
export class TestRunListComponent implements OnInit {
  private readonly svc = inject(TestRunsService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly page = signal(1);
  readonly loading = signal(false);
  readonly result = signal<PagedResult<TestRun>>(emptyPage());

  readonly columns: TableColumn<TestRun>[] = [
    { key: 'testSuiteName', header: 'Suite', value: r => r.testSuiteName ?? r.testSuiteId.slice(0, 8) },
    { key: 'environment', header: 'Environment', type: 'badge' },
    { key: 'status', header: 'Status', type: 'badge' },
    { key: 'passed', header: 'Passed', align: 'right' },
    { key: 'failed', header: 'Failed', align: 'right' },
    { key: 'startedAt', header: 'Started', type: 'datetime' }
  ];

  ngOnInit(): void { this.load(); }

  open(run: TestRun): void { this.router.navigateByUrl(`/test-runs/${run.id}`); }

  private load(): void {
    this.loading.set(true);
    this.svc.list({ page: this.page(), pageSize: 10 })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: r => { this.result.set(r); this.loading.set(false); },
        error: () => { this.result.set(emptyPage()); this.loading.set(false); }
      });
  }
}