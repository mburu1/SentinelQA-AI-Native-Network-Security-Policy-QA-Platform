import { Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '@core/auth/auth.service';

interface NavItem { label: string; path: string; icon: string; roles?: string[]; }
interface NavSection { title: string; items: NavItem[]; }

const NAV: NavSection[] = [
  { title: 'Overview', items: [{ label: 'Dashboard', path: '/dashboard', icon: '📊' }] },
  {
    title: 'Network Security',
    items: [
      { label: 'Firewalls', path: '/firewalls', icon: '🧱' },
      { label: 'Networks', path: '/networks', icon: '🌐' },
      { label: 'Policies', path: '/policies', icon: '📜' },
      { label: 'Change Requests', path: '/change-requests', icon: '🔁' }
    ]
  },
  {
    title: 'Quality Engineering',
    items: [
      { label: 'Test Suites', path: '/test-suites', icon: '🧪' },
      { label: 'Test Runs', path: '/test-runs', icon: '▶️' },
      { label: 'Defects', path: '/defects', icon: '🐞' }
    ]
  },
  {
    title: 'Insights',
    items: [
      { label: 'Audit Trail', path: '/audit', icon: '🕵️', roles: ['Admin', 'SecurityEngineer', 'Approver'] },
      { label: 'Notifications', path: '/notifications', icon: '✉️' }
    ]
  },
  { title: 'AI', items: [{ label: 'AI QA Copilot', path: '/ai-copilot', icon: '✨' }] },
  { title: 'Administration', items: [{ label: 'Administration', path: '/administration', icon: '⚙️', roles: ['Admin'] }] }
];

@Component({
  selector: 'sq-sidebar',
  imports: [RouterLink, RouterLinkActive],
  template: `
    <aside class="sidebar">
      <div class="brand">
        <span class="brand-mark">🛡️</span>
        <div>
          <div class="brand-name">SentinelQA</div>
          <div class="brand-sub">Policy &amp; QA Platform</div>
        </div>
      </div>
      <nav class="nav">
        @for (section of sections(); track section.title) {
          <div class="nav-section">
            <div class="nav-section-title">{{ section.title }}</div>
            @for (item of section.items; track item.path) {
              <a class="nav-item" [routerLink]="item.path" routerLinkActive="active">
                <span class="nav-icon">{{ item.icon }}</span>{{ item.label }}
              </a>
            }
          </div>
        }
      </nav>
      <div class="sidebar-footer">SentinelQA v1.0 · .NET 10 · Angular 20</div>
    </aside>
  `
})
export class SidebarComponent {
  private readonly auth = inject(AuthService);

  readonly sections = computed<NavSection[]>(() =>
    NAV.map(section => ({
      ...section,
      items: section.items.filter(item => !item.roles || item.roles.some(r => this.auth.hasRole(r)))
    })).filter(section => section.items.length > 0)
  );
}