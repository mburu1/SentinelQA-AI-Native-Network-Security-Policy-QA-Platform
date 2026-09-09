import { Routes } from '@angular/router';
import { roleGuard } from '@core/guards/role.guard';
import { AuditListComponent } from './audit-list.component';

export const AUDIT_ROUTES: Routes = [
  { path: '', component: AuditListComponent, canActivate: [roleGuard], data: { roles: ['Admin', 'SecurityEngineer', 'Approver'] } }
];