import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AdminService, AppRole, AppUser, Tenant } from '@core/services/admin.service';
import { ToastService } from '@core/services/toast.service';
import { DataTableComponent, TableColumn } from '@shared/components/data-table.component';
import { PageHeaderComponent } from '@shared/components/page-header.component';

type Tab = 'users' | 'roles' | 'tenants';

@Component({
  selector: 'sq-administration',
  imports: [PageHeaderComponent, DataTableComponent],
  template: `
    <sq-page-header title="Administration" subtitle="Tenants, users and RBAC roles" />

    <div class="tabs">
      <button class="tab" [class.active]="tab() === 'users'" (click)="tab.set('users'); loadUsers()">Users</button>
      <button class="tab" [class.active]="tab() === 'roles'" (click)="tab.set('roles'); loadRoles()">Roles</button>
      <button class="tab" [class.active]="tab() === 'tenants'" (click)="tab.set('tenants'); loadTenants()">Tenants</button>
    </div>

    @switch (tab()) {
      @case ('users') {
        <div class="card">
          <sq-data-table [columns]="userColumns" [rows]="users()" emptyIcon="👥" emptyTitle="No users" />
        </div>
      }
      @case ('roles') {
        <div class="card">
          <sq-data-table [columns]="roleColumns" [rows]="roles()" emptyIcon="🔑" emptyTitle="No roles" />
        </div>
      }
      @case ('tenants') {
        <div class="toolbar">
          <input class="input search-input" placeholder="New tenant name…"
                 [value]="newTenant()" (input)="newTenant.set($any($event.target).value)" />
          <button class="btn btn-primary" (click)="createTenant()">Create tenant</button>
        </div>
        <div class="card">
          <sq-data-table [columns]="tenantColumns" [rows]="tenants()" emptyIcon="🏢" emptyTitle="No tenants" />
        </div>
      }
    }
  `
})
export class AdministrationComponent implements OnInit {
  private readonly svc = inject(AdminService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly tab = signal<Tab>('users');
  readonly users = signal<AppUser[]>([]);
  readonly roles = signal<AppRole[]>([]);
  readonly tenants = signal<Tenant[]>([]);
  readonly newTenant = signal('');

  readonly userColumns: TableColumn<AppUser>[] = [
    { key: 'displayName', header: 'Name' },
    { key: 'email', header: 'Email' },
    { key: 'roles', header: 'Roles', value: u => u.roles.join(', ') },
    { key: 'isActive', header: 'Active', type: 'badge', value: u => (u.isActive ? 'Active' : 'Inactive') }
  ];

  readonly roleColumns: TableColumn<AppRole>[] = [
    { key: 'name', header: 'Role', type: 'badge' },
    { key: 'description', header: 'Description' }
  ];

  readonly tenantColumns: TableColumn<Tenant>[] = [
    { key: 'name', header: 'Tenant' },
    { key: 'isActive', header: 'Status', type: 'badge', value: t => (t.isActive ? 'Active' : 'Inactive') },
    { key: 'createdAt', header: 'Created', type: 'date' }
  ];

  ngOnInit(): void { this.loadUsers(); }

  loadUsers(): void {
    if (this.users().length) return;
    this.svc.users().pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: u => this.users.set(u), error: () => undefined });
  }

  loadRoles(): void {
    if (this.roles().length) return;
    this.svc.roles().pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: r => this.roles.set(r), error: () => undefined });
  }

  loadTenants(): void {
    this.svc.tenants().pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: t => this.tenants.set(t), error: () => undefined });
  }

  createTenant(): void {
    const name = this.newTenant().trim();
    if (!name) return;
    this.svc.createTenant(name).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { this.toast.success(`Tenant "${name}" created.`); this.newTenant.set(''); this.loadTenants(); },
      error: () => this.toast.error('Could not create tenant.')
    });
  }
}