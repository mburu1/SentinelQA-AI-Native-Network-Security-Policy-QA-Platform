import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { PoliciesService } from '@core/services/policies.service';
import { emptyPage, PagedResult, Policy } from '@core/models';
import { DataTableComponent, TableColumn } from '@shared/components/data-table.component';
import { PageHeaderComponent } from '@shared/components/page-header.component';
import { PaginationComponent } from '@shared/components/pagination.component';

@Component({
  selector: 'sq-policy-list',
  imports: [RouterLink, PageHeaderComponent, DataTableComponent, PaginationComponent],
  template: `
    <sq-page-header title="Security Policies" subtitle="Ordered rule sets attached to firewalls — validated before deployment">
      <a actions class="btn btn-primary" routerLink="/policies/new" data-testid="new-policy">+ Create policy</a>
    </sq-page-header>

    <div class="toolbar">
      <input class="input search-input" placeholder="Search policies…" [value]="search()" (input)="onSearch($event)" />
    </div>

    <div class="card">
      <sq-data-table [columns]="columns" [rows]="result().items" [loading]="loading()" clickable
                     (rowClick)="open($event)" emptyIcon="📜" emptyTitle="No policies yet"
                     emptyMessage="Create a policy to start validating firewall rules." />
      <sq-pagination [page]="page()" [totalPages]="result().totalPages" [total]="result().totalCount"
                     (pageChange)="page.set($event); load()" />
    </div>
  `
})
export class PolicyListComponent implements OnInit {
  private readonly svc = inject(PoliciesService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly search = signal('');
  readonly page = signal(1);
  readonly loading = signal(false);
  readonly result = signal<PagedResult<Policy>>(emptyPage());

  readonly columns: TableColumn<Policy>[] = [
    { key: 'name', header: 'Policy' },
    { key: 'environment', header: 'Environment', type: 'badge' },
    { key: 'status', header: 'Status', type: 'badge' },
    { key: 'version', header: 'Version', align: 'right' },
    { key: 'rules', header: 'Rules', align: 'right', value: p => p.rules?.length ?? 0 },
    { key: 'updatedAt', header: 'Updated', type: 'datetime' }
  ];

  ngOnInit(): void { this.load(); }

  onSearch(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
    this.page.set(1);
    this.load();
  }

  open(policy: Policy): void {
    this.router.navigateByUrl(`/policies/${policy.id}`);
  }

  private load(): void {
    this.loading.set(true);
    this.svc.list({ page: this.page(), pageSize: 10, search: this.search() })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: r => { this.result.set(r); this.loading.set(false); },
        error: () => { this.result.set(emptyPage()); this.loading.set(false); }
      });
  }
}