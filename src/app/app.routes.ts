import { Routes } from '@angular/router';
import { authGuard } from '@core/guards/auth.guard';
import { roleGuard } from '@core/guards/role.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/authentication/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    loadComponent: () => import('./layout/shell.component').then(m => m.ShellComponent),
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', loadChildren: () => import('./features/dashboard/dashboard.routes').then(m => m.DASHBOARD_ROUTES) },
      { path: 'firewalls', loadChildren: () => import('./features/firewalls/firewalls.routes').then(m => m.FIREWALLS_ROUTES) },
      { path: 'networks', loadChildren: () => import('./features/networks/networks.routes').then(m => m.NETWORKS_ROUTES) },
      { path: 'policies', loadChildren: () => import('./features/policies/policies.routes').then(m => m.POLICIES_ROUTES) },
      { path: 'change-requests', loadChildren: () => import('./features/change-requests/change-requests.routes').then(m => m.CHANGE_REQUESTS_ROUTES) },
      { path: 'test-suites', loadChildren: () => import('./features/test-suites/test-suites.routes').then(m => m.TEST_SUITES_ROUTES) },
      { path: 'test-runs', loadChildren: () => import('./features/test-runs/test-runs.routes').then(m => m.TEST_RUNS_ROUTES) },
      { path: 'defects', loadChildren: () => import('./features/defects/defects.routes').then(m => m.DEFECTS_ROUTES) },
      { path: 'audit', loadChildren: () => import('./features/audit/audit.routes').then(m => m.AUDIT_ROUTES) },
      { path: 'notifications', loadChildren: () => import('./features/notifications/notifications.routes').then(m => m.NOTIFICATIONS_ROUTES) },
      { path: 'ai-copilot', loadChildren: () => import('./features/ai-copilot/ai-copilot.routes').then(m => m.AI_ROUTES) },
      {
        path: 'administration',
        canActivate: [roleGuard],
        data: { roles: ['Admin'] },
        loadChildren: () => import('./features/administration/administration.routes').then(m => m.ADMINISTRATION_ROUTES)
      },
      { path: 'forbidden', loadComponent: () => import('./features/system/forbidden.component').then(m => m.ForbiddenComponent) }
    ]
  },
  {
    path: '**',
    loadComponent: () => import('./features/system/not-found.component').then(m => m.NotFoundComponent)
  }
];