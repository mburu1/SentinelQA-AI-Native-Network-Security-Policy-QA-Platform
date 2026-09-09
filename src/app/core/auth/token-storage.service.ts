import { Injectable } from '@angular/core';

const REFRESH_KEY = 'sentinelqa.refreshToken';

/**
 * The access token lives only in memory (AuthService).
 * The refresh token is persisted so a browser reload can silently re-authenticate.
 */
@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_KEY);
  }

  setRefreshToken(token: string): void {
    localStorage.setItem(REFRESH_KEY, token);
  }

  hasRefreshToken(): boolean {
    return !!this.getRefreshToken();
  }

  clear(): void {
    localStorage.removeItem(REFRESH_KEY);
  }
}