import { Component, input } from '@angular/core';

@Component({
  selector: 'sq-empty-state',
  template: `
    <div class="empty">
      <div class="empty-icon">{{ icon() }}</div>
      <h3>{{ title() }}</h3>
      <p class="muted">{{ message() }}</p>
      <ng-content />
    </div>
  `
})
export class EmptyStateComponent {
  readonly icon = input('📭');
  readonly title = input('Nothing here yet');
  readonly message = input('');
}