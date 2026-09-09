import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { DefectsService } from '@core/services/defects.service';
import { Defect, DEFECT_STATUSES, emptyPage, PagedResult } from '@core/models';
import { DataTableComponent, TableColumn } from '@shared/components/data-table.component';
import { PageHeaderComponent } from '@shared/components/page-header.component';
import { PaginationComponent } from '@shared/components/pagination.component';

@Component({
  selector: 'sq-defect-list',
  imports: [RouterLink, PageHeaderComponent, DataTableComponent, PaginationComponent],
  template: `
    <sq-page-header title="Defects" subtitle="Open → Triaged → In Progress → Fixed → Retest → Verified → Closed">
      <a actions class="btn btn-primary" routerLink="/defects/new" data-testid="new-defect">+ Report defect</a>
    </sq-page-header>

    <div class="toolbar filters">
      <select class="select" style="width:170px" [value]="status()" (change)="status.set($any($event.target).value); page.set(1); load()">
        <option value="">All statuses</option>
        @for (s of statuses; track s) { <option [value]="s">{{ s }}</option> }
      </select>
      <input class="input search-input" placeholder="Search defects…" [value]="search()" (input)="search.set($any($event.target).value); page.set(1); load()" />
    </div>

    <div class="card">
      <sq-data-table [columns]="columns" [rows]="result().items" [loading]="loading()" clickable
                     (rowClick)="open($event)" emptyIcon="🐞" emptyTitle="No defects found"
                     emptyMessage="Either everything works — or you need more tests." />
      <sq-pagination [page]="page()" [totalPages]="result().totalPages" [total]="result().totalCount"
                     (pageChange)="page.set($event); load()" />
    </div>
  `
})
export class DefectListComponent implements OnInit {
  private readonly svc = inject(DefectsService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly statuses = DEFECT_STATUSES;
  readonly page = signal(1);
  readonly status = signal('');
  readonly search = signal('');
  readonly loading = signal(false);
  readonly result = signal<PagedResult<Defect>>(emptyPage());

  readonly columns: TableColumn<Defect>[] = [
    { key: 'title', header: 'Title' },
    { key: 'severity', header: 'Severity', type: 'badge' },
    { key: 'priority', header: 'Priority', type: 'badge' },
    { key: 'status', header: 'Status', type: 'badge' },
    { key: 'environment', header: 'Env', type: 'badge' },
    { key: 'assigneeName', header: 'Assignee', value: d => d.assigneeName ?? '—' },
    { key: 'updatedAt', header: 'Updated', type: 'datetime' }
  ];

  ngOnInit(): void { this.load(); }

  open(defect: Defect): void { this.router.navigateByUrl(`/defects/${defect.id}`); }

  private load(): void {
    this.loading.set(true);
    this.svc.list({ page: this.page(), pageSize: 10, search: this.search(), status: this.status() || undefined })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: r => { this.result.set(r); this.loading.set(false); },
        error: () => { this.result.set(emptyPage()); this.loading.set(false); }
      });
  }
}