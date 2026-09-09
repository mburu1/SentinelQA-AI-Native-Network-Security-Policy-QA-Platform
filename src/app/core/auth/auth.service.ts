import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, map, Observable, of, tap, throwError } from 'rxjs';
import { environment } from '@env/environment';
import { LoginResponse, UserInfo } from '@core/models';
import { TokenStorageService } from './token-storage.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly tokens = inject(TokenStorageService);

  private readonly _user = signal<UserInfo | null>(null);
  private readonly _accessToken = signal<string | null>(null);

  readonly user = this._user.asReadonly();
  readonly isAuthenticated = computed(() => this._accessToken() !== null);

  accessToken(): string | null {
    return this._accessToken();
  }

  hasRefreshToken(): boolean {
    return this.tokens.hasRefreshToken();
  }

  roles(): string[] {
    return this._user()?.roles ?? [];
  }

  hasRole(...roles: string[]): boolean {
    const mine = this.roles();
    return roles.some(r => mine.includes(r));
  }

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${environment.apiUrl}/auth/login`, { email, password })
      .pipe(tap(r => this.apply(r)));
  }

  refresh(): Observable<LoginResponse> {
    const refreshToken = this.tokens.getRefreshToken();
    if (!refreshToken) return throwError(() => new Error('No refresh token'));
    return this.http
      .post<LoginResponse>(`${environment.apiUrl}/auth/refresh`, { refreshToken })
      .pipe(
        tap(r => this.apply(r)),
        catchError(err => {
          this.clearSession();
          return throwError(() => err);
        })
      );
  }

  /** Used by the auth guard after a reload: silently re-authenticate. */
  restoreSession(): Observable<boolean> {
    if (this.isAuthenticated()) return of(true);
    if (!this.tokens.hasRefreshToken()) return of(false);
    return this.refresh().pipe(
      map(() => true),
      catchError(() => of(false))
    );
  }

  logout(): void {
    const refreshToken = this.tokens.getRefreshToken();
    if (refreshToken) {
      this.http
        .post(`${environment.apiUrl}/auth/logout`, { refreshToken })
        .subscribe({ error: () => undefined });
    }
    this.clearSession();
    this.router.navigateByUrl('/login');
  }

  private apply(r: LoginResponse): void {
    this._accessToken.set(r.accessToken);
    this._user.set(r.user);
    this.tokens.setRefreshToken(r.refreshToken);
  }

  private clearSession(): void {
    this._accessToken.set(null);
    this._user.set(null);
    this.tokens.clear();
  }
}