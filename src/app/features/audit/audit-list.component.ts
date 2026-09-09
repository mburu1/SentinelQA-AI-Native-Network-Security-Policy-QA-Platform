import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuditService } from '@core/services/audit.service';
import { AuditEntry, emptyPage, PagedResult } from '@core/models';
import { DataTableComponent, TableColumn } from '@shared/components/data-table.component';
import { PageHeaderComponent } from '@shared/components/page-header.component';
import { PaginationComponent } from '@shared/components/pagination.component';

@Component({
  selector: 'sq-audit-list',
  imports: [PageHeaderComponent, DataTableComponent, PaginationComponent],
  template: `
    <sq-page-header title="Audit Trail" subtitle="Immutable security event log — served with Cache-Control: no-store" />

    <div class="toolbar">
      <input class="input search-input" placeholder="Filter by action, user, resource…"
             [value]="search()" (input)="search.set($any($event.target).value); page.set(1); load()" />
    </div>

    <div class="card">
      <sq-data-table [columns]="columns" [rows]="result().items" [loading]="loading()"
                     emptyIcon="🕵️" emptyTitle="No audit entries" />
      <sq-pagination [page]="page()" [totalPages]="result().totalPages" [total]="result().totalCount"
                     (pageChange)="page.set($event); load()" />
    </div>
  `
})
export class AuditListComponent implements OnInit {
  private readonly svc = inject(AuditService);
  private readonly destroyRef = inject(DestroyRef);

  readonly page = signal(1);
  readonly search = signal('');
  readonly loading = signal(false);
  readonly result = signal<PagedResult<AuditEntry>>(emptyPage());

  readonly columns: TableColumn<AuditEntry>[] = [
    { key: 'createdAt', header: 'Timestamp', type: 'datetime' },
    { key: 'userName', header: 'User' },
    { key: 'action', header: 'Action', type: 'badge' },
    { key: 'resource', header: 'Resource' },
    { key: 'resourceId', header: 'Resource ID', type: 'mono', value: e => e.resourceId?.slice(0, 13) ?? '—' },
    { key: 'traceId', header: 'Trace', type: 'mono', value: e => e.traceId?.slice(0, 13) ?? '—' }
  ];

  ngOnInit(): void { this.load(); }

  private load(): void {
    this.loading.set(true);
    this.svc.list({ page: this.page(), pageSize: 15, search: this.search() })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: r => { this.result.set(r); this.loading.set(false); },
        error: () => { this.result.set(emptyPage()); this.loading.set(false); }
      });
  }
}