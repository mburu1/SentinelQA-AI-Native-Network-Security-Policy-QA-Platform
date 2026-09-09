import {
  HttpErrorResponse,
  HttpHandlerFn,
  HttpInterceptorFn,
  HttpRequest
} from '@angular/common/http';
import { inject } from '@angular/core';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { AuthService } from '@core/auth/auth.service';

let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

function isAuthEndpoint(req: HttpRequest<unknown>): boolean {
  return req.url.includes('/auth/login') || req.url.includes('/auth/refresh') || req.url.includes('/auth/logout');
}

function addToken(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}

function handle401(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  auth: AuthService
) {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);
    return auth.refresh().pipe(
      switchMap(response => {
        isRefreshing = false;
        refreshTokenSubject.next(response.accessToken);
        return next(addToken(req, response.accessToken));
      }),
      catchError(err => {
        isRefreshing = false;
        auth.logout();
        return throwError(() => err);
      })
    );
  }

  return refreshTokenSubject.pipe(
    filter(token => token !== null),
    take(1),
    switchMap(token => next(addToken(req, token as string)))
  );
}

/** Attaches the JWT and performs single-flight refresh on 401. */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  if (!req.url.startsWith('/api') || isAuthEndpoint(req)) {
    return next(req);
  }

  const token = auth.accessToken();
  const request = token ? addToken(req, token) : req;

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && auth.hasRefreshToken()) {
        return handle401(req, next, auth);
      }
      return throwError(() => error);
    })
  );
};