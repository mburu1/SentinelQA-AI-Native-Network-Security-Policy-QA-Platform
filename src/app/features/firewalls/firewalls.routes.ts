import { Routes } from '@angular/router';
import { roleGuard } from '@core/guards/role.guard';
import { FirewallDetailComponent } from './firewall-detail.component';
import { FirewallFormComponent } from './firewall-form.component';
import { FirewallListComponent } from './firewall-list.component';

export const FIREWALLS_ROUTES: Routes = [
  { path: '', component: FirewallListComponent },
  {
    path: 'new',
    component: FirewallFormComponent,
    canActivate: [roleGuard],
    data: { roles: ['Admin', 'SecurityEngineer'] }
  },
  { path: ':id', component: FirewallDetailComponent },
  {
    path: ':id/edit',
    component: FirewallFormComponent,
    canActivate: [roleGuard],
    data: { roles: ['Admin', 'SecurityEngineer'] }
  }
];