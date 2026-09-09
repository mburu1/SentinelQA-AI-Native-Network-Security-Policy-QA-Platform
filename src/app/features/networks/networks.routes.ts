import { Routes } from '@angular/router';
import { roleGuard } from '@core/guards/role.guard';
import { NetworkFormComponent } from './network-form.component';
import { NetworkListComponent } from './network-list.component';

export const NETWORKS_ROUTES: Routes = [
  { path: '', component: NetworkListComponent },
  { path: 'new', component: NetworkFormComponent, canActivate: [roleGuard], data: { roles: ['Admin', 'SecurityEngineer'] } }
];