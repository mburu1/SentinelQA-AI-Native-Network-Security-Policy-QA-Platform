import { Directive, inject, Input, OnChanges, TemplateRef, ViewContainerRef } from '@angular/core';
import { AuthService } from '@core/auth/auth.service';

/** Structural directive: renders content only when the current user has any of the roles.
 *  Usage: <button *sqHasRole="'Admin'">…</button>  or  *sqHasRole="['Admin','Approver']"
 */
@Directive({ selector: '[sqHasRole]' })
export class HasRoleDirective implements OnChanges {
  @Input() sqHasRole: string | string[] = [];

  private readonly template = inject(TemplateRef<unknown>);
  private readonly vcr = inject(ViewContainerRef);
  private readonly auth = inject(AuthService);

  ngOnChanges(): void {
    const roles = Array.isArray(this.sqHasRole) ? this.sqHasRole : [this.sqHasRole];
    if (this.auth.hasRole(...roles)) {
      this.vcr.createEmbeddedView(this.template);
    } else {
      this.vcr.clear();
    }
  }
}