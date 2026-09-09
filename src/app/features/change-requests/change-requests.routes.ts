import { Routes } from '@angular/router';
import { roleGuard } from '@core/guards/role.guard';
import { ChangeRequestDetailComponent } from './change-request-detail.component';
import { ChangeRequestFormComponent } from './change-request-form.component';
import { ChangeRequestListComponent } from './change-request-list.component';

export const CHANGE_REQUESTS_ROUTES: Routes = [
  { path: '', component: ChangeRequestListComponent },
  {
    path: 'new',
    component: ChangeRequestFormComponent,
    canActivate: [roleGuard],
    data: { roles: ['Admin', 'SecurityEngineer', 'Developer'] }
  },
  { path: ':id', component: ChangeRequestDetailComponent }
];