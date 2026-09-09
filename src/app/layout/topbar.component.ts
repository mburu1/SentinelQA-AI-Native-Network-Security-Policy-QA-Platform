import { DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { environment } from '@env/environment';
import { AuthService } from '@core/auth/auth.service';
import { HealthService } from '@core/services/health.service';
import { InitialsPipe } from '@shared/pipes/common.pipes';

@Component({
  selector: 'sq-topbar',
  imports: [DatePipe, InitialsPipe],
  template: `
    <header class="topbar">
      <div class="topbar-left">
        <span class="env-chip">{{ envName }}</span>
        <span class="muted small">{{ today | date: 'EEEE, d MMMM yyyy' }}</span>
      </div>
      <div class="topbar-right">
        <span class="health" [class.ok]="health() === 'Healthy'" [class.bad]="health() === 'Unhealthy'">
          <span class="health-dot"></span> API {{ health() }}
        </span>
        <div class="user-menu" (click)="menuOpen.set(!menuOpen())">
          <span class="avatar">{{ auth.user()?.displayName | initials }}</span>
          <span>{{ auth.user()?.displayName }}</span>
          @if (menuOpen()) {
            <div class="user-dropdown" (click)="$event.stopPropagation()">
              <div class="user-email">{{ auth.user()?.email }}</div>
              <div class="user-roles">
                @for (role of auth.user()?.roles; track role) { <span class="chip">{{ role }}</span> }
              </div>
              <button class="btn btn-ghost btn-sm" (click)="auth.logout()" data-testid="logout">Sign out</button>
            </div>
          }
        </div>
      </div>
    </header>
  `
})
export class TopbarComponent implements OnInit {
  readonly auth = inject(AuthService);
  private readonly healthService = inject(HealthService);
  private readonly destroyRef = inject(DestroyRef);

  readonly envName = environment.environmentName;
  readonly today = new Date();
  readonly menuOpen = signal(false);
  readonly health = signal<string>('Checking…');

  ngOnInit(): void {
    this.pollHealth();
  }

  private pollHealth(): void {
    const poll = () =>
      this.healthService.live().subscribe({
        next: status => this.health.set(status.status),
        error: () => this.health.set('Unhealthy')
      });

    poll();
    const interval = setInterval(poll, 30_000);
    this.destroyRef.onDestroy(() => clearInterval(interval));
  }
}