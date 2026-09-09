import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { FirewallsService } from '@core/services/firewalls.service';
import { emptyPage, Firewall, PagedResult } from '@core/models';
import { DataTableComponent, TableColumn } from '@shared/components/data-table.component';
import { PageHeaderComponent } from '@shared/components/page-header.component';
import { PaginationComponent } from '@shared/components/pagination.component';

@Component({
  selector: 'sq-firewall-list',
  imports: [RouterLink, PageHeaderComponent, DataTableComponent, PaginationComponent],
  template: `
    <sq-page-header title="Firewalls" subtitle="Managed firewall inventory across all environments">
      <a actions class="btn btn-primary" routerLink="/firewalls/new" data-testid="new-firewall">+ Register firewall</a>
    </sq-page-header>

    <div class="toolbar">
      <input class="input search-input" placeholder="Search by name, vendor, environment…"
             [value]="search()" (input)="onSearch($event)" />
    </div>

    <div class="card">
      <sq-data-table [columns]="columns" [rows]="result().items" [loading]="loading()" clickable
                     (rowClick)="open($event)" emptyIcon="🧱" emptyTitle="No firewalls registered"
                     emptyMessage="Register your first firewall to start managing policies." />
      <sq-pagination [page]="page()" [totalPages]="result().totalPages" [total]="result().totalCount"
                     (pageChange)="setPage($event)" />
    </div>
  `
})
export class FirewallListComponent implements OnInit {
  private readonly svc = inject(FirewallsService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly search = signal('');
  readonly page = signal(1);
  readonly loading = signal(false);
  readonly result = signal<PagedResult<Firewall>>(emptyPage());

  readonly columns: TableColumn<Firewall>[] = [
    { key: 'name', header: 'Firewall' },
    { key: 'vendor', header: 'Vendor' },
    { key: 'environment', header: 'Environment', type: 'badge' },
    { key: 'status', header: 'Status', type: 'badge' },
    { key: 'ipAddress', header: 'IP Address', type: 'mono' },
    { key: 'lastHealthCheckAt', header: 'Last Health Check', type: 'datetime' }
  ];

  ngOnInit(): void { this.load(); }

  onSearch(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
    this.page.set(1);
    this.load();
  }

  setPage(page: number): void {
    this.page.set(page);
    this.load();
  }

  open(firewall: Firewall): void {
    this.router.navigateByUrl(`/firewalls/${firewall.id}`);
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