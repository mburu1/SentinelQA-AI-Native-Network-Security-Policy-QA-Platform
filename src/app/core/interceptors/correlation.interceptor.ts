import { HttpInterceptorFn } from '@angular/common/http';

/** Adds a correlation id to every outbound request for distributed tracing. */
export const correlationInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.startsWith('/api')) return next(req);
  const request = req.clone({ setHeaders: { 'X-Correlation-Id': crypto.randomUUID() } });
  return next(request);
};