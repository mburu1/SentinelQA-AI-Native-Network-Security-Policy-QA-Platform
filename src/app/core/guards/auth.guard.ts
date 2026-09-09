import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from '@core/auth/auth.service';

export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated()) return true;

  if (auth.hasRefreshToken()) {
    return auth.restoreSession().pipe(
      map(ok =>
        ok ? true : router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } })
      )
    );
  }

  return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};