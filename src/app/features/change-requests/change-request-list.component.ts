import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { ChangeRequestsService } from '@core/services/change-requests.service';
import { ChangeRequest, emptyPage, PagedResult } from '@core/models';
import { DataTableComponent, TableColumn } from '@shared/components/data-table.component';
import { PageHeaderComponent } from '@shared/components/page-header.component';
import { PaginationComponent } from '@shared/components/pagination.component';

@Component({
  selector: 'sq-change-request-list',
  imports: [RouterLink, PageHeaderComponent, DataTableComponent, PaginationComponent],
  template: `
    <sq-page-header title="Change Requests"
      subtitle="Draft → Submitted → Validated → Testing → Approved → Deployment → Verification → Completed">
      <a actions class="btn btn-primary" routerLink="/change-requests/new" data-testid="new-change-request">+ New change request</a>
    </sq-page-header>

    <div class="card">
      <sq-data-table [columns]="columns" [rows]="result().items" [loading]="loading()" clickable
                     (rowClick)="open($event)" emptyIcon="🔁" emptyTitle="No change requests" />
      <sq-pagination [page]="page()" [totalPages]="result().totalPages" [total]="result().totalCount"
                     (pageChange)="page.set($event); load()" />
    </div>
  `
})
export class ChangeRequestListComponent implements OnInit {
  private readonly svc = inject(ChangeRequestsService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly page = signal(1);
  readonly loading = signal(false);
  readonly result = signal<PagedResult<ChangeRequest>>(emptyPage());

  readonly columns: TableColumn<ChangeRequest>[] = [
    { key: 'title', header: 'Title' },
    { key: 'policyName', header: 'Policy', value: cr => cr.policyName ?? cr.policyId.slice(0, 8) },
    { key: 'state', header: 'State', type: 'badge' },
    { key: 'risk', header: 'Risk', type: 'badge' },
    { key: 'requestedByName', header: 'Requested By', value: cr => cr.requestedByName ?? '—' },
    { key: 'updatedAt', header: 'Updated', type: 'datetime' }
  ];

  ngOnInit(): void { this.load(); }

  open(cr: ChangeRequest): void { this.router.navigateByUrl(`/change-requests/${cr.id}`); }

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