import { Component, input } from '@angular/core';

@Component({
  selector: 'sq-spinner',
  template: `<span class="spinner" [class.spinner-lg]="large()" role="status" [attr.aria-label]="label()"></span>`
})
export class SpinnerComponent {
  readonly large = input(false);
  readonly label = input('Loading');
}