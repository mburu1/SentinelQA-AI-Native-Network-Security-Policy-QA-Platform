import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ApiError, ProblemDetails } from '@core/models';
import { ToastService } from '@core/services/toast.service';

function normalize(error: HttpErrorResponse): ApiError {
  const body = error.error as ProblemDetails | undefined;
  const traceId = body?.traceId ?? error.headers.get('traceparent') ?? undefined;
  return new ApiError(
    error.status,
    body?.title ?? error.statusText ?? 'Request failed',
    body?.detail,
    body?.errors,
    traceId
  );
}

/** Normalizes ProblemDetails to ApiError; toasts only server-side failures (5xx / network). */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const apiError = normalize(error);
      if (apiError.status >= 500 || apiError.status === 0) {
        toast.error(`${apiError.title}${apiError.traceId ? ` (trace: ${apiError.traceId.slice(0, 8)}…)` : ''}`);
      }
      return throwError(() => apiError);
    })
  );
};