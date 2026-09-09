import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { TestSuitesService } from '@core/services/test-suites.service';
import { ToastService } from '@core/services/toast.service';
import { emptyPage, PagedResult, TestSuite } from '@core/models';
import { DataTableComponent, TableColumn } from '@shared/components/data-table.component';
import { PageHeaderComponent } from '@shared/components/page-header.component';
import { PaginationComponent } from '@shared/components/pagination.component';
import { StatusBadgeComponent } from '@shared/components/status-badge.component';

@Component({
  selector: 'sq-test-suite-list',
  imports: [RouterLink, PageHeaderComponent, DataTableComponent, PaginationComponent, StatusBadgeComponent],
  template: `
    <sq-page-header title="Test Suites" subtitle="Regression packs and functional suites executed by the test runner">
      <button actions class="btn btn-primary" (click)="showCreate.set(!showCreate())">+ New suite</button>
    </sq-page-header>

    @if (showCreate()) {
      <div class="card"><div class="card-body" style="display:flex;gap:12px;flex-wrap:wrap;align-items:end">
        <div class="form-field" style="flex:2;min-width:220px;margin:0">
          <label class="label">Name</label>
          <input class="input" [value]="newName()" (input)="newName.set($any($event.target).value)" placeholder="policy-validation-regression" />
        </div>
        <label class="checkbox-row" style="margin:0 0 6px">
          <input type="checkbox" [checked]="newRegression()" (change)="newRegression.set($any($event.target).checked)" />
          Regression suite
        </label>
        <button class="btn btn-primary" (click)="create()">Create</button>
      </div></div>
    }

    <div class="card">
      <sq-data-table [columns]="columns" [rows]="result().items" [loading]="loading()" clickable
                     (rowClick)="open($event)" emptyIcon="🧪" emptyTitle="No test suites" />
      <sq-pagination [page]="page()" [totalPages]="result().totalPages" [total]="result().totalCount"
                     (pageChange)="page.set($event); load()" />
    </div>
  `
})
export class TestSuiteListComponent implements OnInit {
  private readonly svc = inject(TestSuitesService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly page = signal(1);
  readonly loading = signal(false);
  readonly showCreate = signal(false);
  readonly newName = signal('');
  readonly newRegression = signal(false);
  readonly result = signal<PagedResult<TestSuite>>(emptyPage());

  readonly columns: TableColumn<TestSuite>[] = [
    { key: 'name', header: 'Suite' },
    { key: 'isRegression', header: 'Kind', value: s => (s.isRegression ? 'Regression' : 'Functional'), type: 'badge' },
    { key: 'environment', header: 'Environment', type: 'badge' },
    { key: 'caseCount', header: 'Cases', align: 'right' },
    { key: 'updatedAt', header: 'Updated', type: 'datetime' }
  ];

  ngOnInit(): void { this.load(); }

  open(suite: TestSuite): void { this.router.navigateByUrl(`/test-suites/${suite.id}`); }

  create(): void {
    const name = this.newName().trim();
    if (!name) return;
    this.svc.create({ name, isRegression: this.newRegression() })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: suite => {
          this.toast.success(`Suite "${suite.name}" created.`);
          this.showCreate.set(false);
          this.newName.set('');
          this.load();
        },
        error: () => this.toast.error('Could not create suite.')
      });
  }

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