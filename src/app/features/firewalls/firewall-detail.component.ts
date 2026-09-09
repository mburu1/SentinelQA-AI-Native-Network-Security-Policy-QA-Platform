import { DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, input, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { FirewallsService } from '@core/services/firewalls.service';
import { PoliciesService } from '@core/services/policies.service';
import { ConfirmService } from '@core/services/confirm.service';
import { ToastService } from '@core/services/toast.service';
import { Firewall, Policy } from '@core/models';
import { HasRoleDirective } from '@shared/directives/has-role.directive';
import { DataTableComponent, TableColumn } from '@shared/components/data-table.component';
import { PageHeaderComponent } from '@shared/components/page-header.component';
import { SpinnerComponent } from '@shared/components/spinner.component';
import { StatusBadgeComponent } from '@shared/components/status-badge.component';

@Component({
  selector: 'sq-firewall-detail',
  imports: [RouterLink, DatePipe, HasRoleDirective, PageHeaderComponent, SpinnerComponent,
            StatusBadgeComponent, DataTableComponent],
  template: `
    @if (loading()) {
      <div class="page-loading"><sq-spinner large /></div>
    } @else if (firewall(); as fw) {
      <sq-page-header [title]="fw.name" [subtitle]="fw.vendor + ' · ' + fw.environment">
        <a actions class="btn" [routerLink]="'/firewalls/' + fw.id + '/edit'" *sqHasRole="['Admin','SecurityEngineer']">Edit</a>
        <button actions class="btn btn-danger" (click)="remove(fw)" *sqHasRole="'Admin'">Delete</button>
      </sq-page-header>

      <div class="grid-2">
        <div class="card">
          <div class="card-head"><span class="card-title">Details</span></div>
          <div class="card-body">
            <div class="kv">
              <span class="kv-key">Status</span><span><sq-status-badge [label]="fw.status" /></span>
              <span class="kv-key">Vendor</span><span class="kv-val">{{ fw.vendor }}</span>
              <span class="kv-key">Environment</span><span class="kv-val">{{ fw.environment }}</span>
              <span class="kv-key">IP Address</span><span class="kv-val mono">{{ fw.ipAddress ?? '—' }}</span>
              <span class="kv-key">Model</span><span class="kv-val">{{ fw.model ?? '—' }}</span>
              <span class="kv-key">Firmware</span><span class="kv-val">{{ fw.firmwareVersion ?? '—' }}</span>
              <span class="kv-key">Last health check</span><span class="kv-val">{{ fw.lastHealthCheckAt | date: 'medium' }}</span>
              <span class="kv-key">Created</span><span class="kv-val">{{ fw.createdAt | date: 'medium' }}</span>
            </div>
          </div>
        </div>

        <div class="card">
          <div class="card-head"><span class="card-title">Policies on this firewall</span>
            <a class="btn btn-ghost btn-sm" routerLink="/policies/new">+ New policy</a></div>
          <sq-data-table [columns]="policyColumns" [rows]="policies()" clickable
                         (rowClick)="goPolicy($event)" emptyIcon="📜" emptyTitle="No policies yet" />
        </div>
      </div>
    }
  `
})
export class FirewallDetailComponent implements OnInit {
  readonly id = input.required<string>();

  private readonly svc = inject(FirewallsService);
  private readonly policies = inject(PoliciesService);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly firewall = signal<Firewall | null>(null);

  readonly policyColumns: TableColumn<Policy>[] = [
    { key: 'name', header: 'Policy' },
    { key: 'status', header: 'Status', type: 'badge' },
    { key: 'version', header: 'Version', align: 'right' },
    { key: 'updatedAt', header: 'Updated', type: 'datetime' }
  ];

  ngOnInit(): void {
    const id = this.id();
    this.svc.get(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: fw => { this.firewall.set(fw); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
    this.policies.list({ pageSize: 100, firewallId: id }).pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: r => this.policies$.set(r.items.filter(p => p.firewallId === id)) });
  }

  readonly policies$ = signal<Policy[]>([]);
  policies() { return this.policies$(); }

  goPolicy(policy: Policy): void {
    this.router.navigateByUrl(`/policies/${policy.id}`);
  }

  async remove(fw: Firewall): Promise<void> {
    const ok = await this.confirm.confirm({
      title: 'Delete firewall',
      message: `Permanently delete "${fw.name}"? Connected policies become orphaned.`,
      confirmLabel: 'Delete',
      danger: true
    });
    if (!ok) return;
    this.svc.remove(fw.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { this.toast.success('Firewall deleted.'); this.router.navigateByUrl('/firewalls'); },
      error: () => this.toast.error('Delete failed. Firewall may have dependent policies.')
    });
  }
}