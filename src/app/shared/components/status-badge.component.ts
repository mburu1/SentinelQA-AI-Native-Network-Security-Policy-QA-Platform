import { Component, computed, input } from '@angular/core';

const TONES: Record<string, string[]> = {
  ok: ['Active', 'Completed', 'Passed', 'Verified', 'Approved', 'Healthy', 'Allow', 'Sent', 'Delivered', 'Success'],
  warn: ['Running', 'InProgress', 'Validating', 'Testing', 'Deploying', 'Verification',
         'AwaitingApproval', 'Submitted', 'Retest', 'Fixed', 'RolledBack', 'Degraded',
         'Pending', 'Queued', 'High', 'Medium', 'Maintenance', 'UnderReview'],
  bad: ['Failed', 'Rejected', 'Unreachable', 'Inactive', 'Error', 'Cancelled', 'Critical',
        'Blocked', 'Expired', 'Open', 'Deny'],
  muted: ['Closed', 'Skipped', 'Archived', 'Deprecated', 'Draft', 'Low', 'Viewer'],
  purple: ['Security', 'AI']
};

@Component({
  selector: 'sq-status-badge',
  template: `<span class="badge" [class.ok]="tone() === 'ok'" [class.warn]="tone() === 'warn'"
                   [class.bad]="tone() === 'bad'" [class.muted]="tone() === 'muted'"
                   [class.purple]="tone() === 'purple'" [class.info]="tone() === 'info'">{{ label() }}</span>`,
  styles: [`
    .badge { display: inline-flex; align-items: center; gap: 6px; padding: 2px 10px;
             border-radius: 999px; font-size: 12px; font-weight: 600; letter-spacing: .2px; white-space: nowrap; }
    .badge::before { content: ''; width: 6px; height: 6px; border-radius: 50%; background: currentColor; }
    .ok { background: var(--ok-soft); color: var(--ok); }
    .warn { background: var(--warn-soft); color: var(--warn); }
    .bad { background: var(--bad-soft); color: var(--bad); }
    .muted { background: var(--panel-2); color: var(--text-muted); }
    .purple { background: var(--purple-soft); color: var(--purple); }
    .info { background: var(--info-soft); color: var(--info); }
  `]
})
export class StatusBadgeComponent {
  readonly label = input.required<string>();

  readonly tone = computed(() => {
    const value = this.label();
    for (const [tone, values] of Object.entries(TONES)) {
      if (values.includes(value)) return tone;
    }
    return 'info';
  });
}