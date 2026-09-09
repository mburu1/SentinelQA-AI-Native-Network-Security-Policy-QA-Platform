import { Routes } from '@angular/router';
import { roleGuard } from '@core/guards/role.guard';
import { PolicyDetailComponent } from './policy-detail.component';
import { PolicyFormComponent } from './policy-form.component';
import { PolicyListComponent } from './policy-list.component';

export const POLICIES_ROUTES: Routes = [
  { path: '', component: PolicyListComponent },
  { path: 'new', component: PolicyFormComponent, canActivate: [roleGuard], data: { roles: ['Admin', 'SecurityEngineer', 'Developer'] } },
  { path: ':id', component: PolicyDetailComponent },
  { path: ':id/edit', component: PolicyFormComponent, canActivate: [roleGuard], data: { roles: ['Admin', 'SecurityEngineer', 'Developer'] } }
];