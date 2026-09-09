import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { NetworksService } from '@core/services/networks.service';
import { emptyPage, Network, NetworkGroup, PagedResult, ServiceDefinition } from '@core/models';
import { DataTableComponent, TableColumn } from '@shared/components/data-table.component';
import { PageHeaderComponent } from '@shared/components/page-header.component';
import { PaginationComponent } from '@shared/components/pagination.component';
import { StatusBadgeComponent } from '@shared/components/status-badge.component';

type Tab = 'networks' | 'groups' | 'services';

@Component({
  selector: 'sq-network-list',
  imports: [RouterLink, PageHeaderComponent, DataTableComponent, PaginationComponent, StatusBadgeComponent],
  template: `
    <sq-page-header title="Networks" subtitle="Address spaces, groups and service definitions used by policy rules">
      <a actions class="btn btn-primary" routerLink="/networks/new">+ Define network</a>
    </sq-page-header>

    <div class="tabs">
      <button class="tab" [class.active]="tab() === 'networks'" (click)="tab.set('networks')">Networks</button>
      <button class="tab" [class.active]="tab() === 'groups'" (click)="tab.set('groups'); loadGroups()">Network Groups</button>
      <button class="tab" [class.active]="tab() === 'services'" (click)="tab.set('services'); loadServices()">Services</button>
    </div>

    <div class="card">
      @switch (tab()) {
        @case ('networks') {
          <sq-data-table [columns]="networkColumns" [rows]="networks().items" [loading]="loading()"
                         emptyIcon="🌐" emptyTitle="No networks defined" />
          <sq-pagination [page]="page()" [totalPages]="networks().totalPages" [total]="networks().totalCount"
                         (pageChange)="page.set($event); load()" />
        }
        @case ('groups') {
          <sq-data-table [columns]="groupColumns" [rows]="groups()"
                         emptyIcon="🗂️" emptyTitle="No network groups" />
        }
        @case ('services') {
          <sq-data-table [columns]="serviceColumns" [rows]="services()"
                         emptyIcon="🛰️" emptyTitle="No services defined" />
        }
      }
    </div>
  `
})
export class NetworkListComponent implements OnInit {
  private readonly svc = inject(NetworksService);
  private readonly destroyRef = inject(DestroyRef);

  readonly tab = signal<Tab>('networks');
  readonly page = signal(1);
  readonly loading = signal(false);
  readonly networks = signal<PagedResult<Network>>(emptyPage());
  readonly groups = signal<NetworkGroup[]>([]);
  readonly services = signal<ServiceDefinition[]>([]);

  readonly networkColumns: TableColumn<Network>[] = [
    { key: 'name', header: 'Name' },
    { key: 'cidr', header: 'CIDR', type: 'mono' },
    { key: 'type', header: 'Type', type: 'badge' },
    { key: 'description', header: 'Description' }
  ];

  readonly groupColumns: TableColumn<NetworkGroup>[] = [
    { key: 'name', header: 'Group' },
    { key: 'description', header: 'Description' },
    { key: 'networkIds', header: 'Members', value: g => g.networkIds.length }
  ];

  readonly serviceColumns: TableColumn<ServiceDefinition>[] = [
    { key: 'name', header: 'Service' },
    { key: 'protocol', header: 'Protocol', type: 'badge' },
    { key: 'port', header: 'Port', type: 'mono', align: 'right' },
    { key: 'description', header: 'Description' }
  ];

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.list({ page: this.page(), pageSize: 10 })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: r => { this.networks.set(r); this.loading.set(false); },
        error: () => this.loading.set(false)
      });
  }

  loadGroups(): void {
    if (this.groups().length) return;
    this.svc.groups().pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: g => this.groups.set(g), error: () => undefined });
  }

  loadServices(): void {
    if (this.services().length) return;
    this.svc.services().pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: s => this.services.set(s), error: () => undefined });
  }
}