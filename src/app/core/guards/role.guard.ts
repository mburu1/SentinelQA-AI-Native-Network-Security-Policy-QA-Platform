import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '@core/auth/auth.service';

/** Usage: data: { roles: ['Admin', 'SecurityEngineer'] } — any role grants access. */
export const roleGuard: CanActivateFn = route => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const required = (route.data['roles'] as string[] | undefined) ?? [];

  if (required.length === 0 || required.some(r => auth.hasRole(r))) return true;
  return router.createUrlTree(['/forbidden']);
};