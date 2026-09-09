import { Component, input } from '@angular/core';

@Component({
  selector: 'sq-page-header',
  template: `
    <div class="page-head">
      <div>
        <h1 class="page-title">{{ title() }}</h1>
        @if (subtitle()) { <p class="page-sub">{{ subtitle() }}</p> }
      </div>
      <div class="page-actions"><ng-content select="[actions]" /></div>
    </div>
  `
})
export class PageHeaderComponent {
  readonly title = input.required<string>();
  readonly subtitle = input<string>();
}