import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DefectsService } from '@core/services/defects.service';
import { ToastService } from '@core/services/toast.service';
import {
  ApiError, CreateDefectRequest, DEFECT_PRIORITIES, DefectPriority,
  ENVIRONMENTS, SEVERITIES, Severity
} from '@core/models';
import { PageHeaderComponent } from '@shared/components/page-header.component';

@Component({
  selector: 'sq-defect-form',
  imports: [ReactiveFormsModule, RouterLink, PageHeaderComponent],
  template: `
    <sq-page-header title="Report Defect" subtitle="Include evidence: steps, expected/actual, trace id" />

    @if (error(); as err) { <div class="alert alert-error">{{ err }}</div> }
    @if (fromAi()) { <div class="alert alert-info">✨ Pre-filled by AI QA Copilot — verify before submitting.</div> }

    <div class="card"><div class="card-body" style="max-width:760px">
      <form [formGroup]="form" (ngSubmit)="submit()">
        <div class="form-field">
          <label class="label">Title</label>
          <input class="input" formControlName="title" placeholder="Public SSH rule incorrectly accepted" />
        </div>
        <div class="form-field">
          <label class="label">Description</label>
          <textarea class="textarea" formControlName="description"></textarea>
        </div>
        <div class="form-grid">
          <div class="form-field"><label class="label">Severity</label>
            <select class="select" formControlName="severity">
              @for (s of severities; track s) { <option [value]="s">{{ s }}</option> }
            </select></div>
          <div class="form-field"><label class="label">Priority</label>
            <select class="select" formControlName="priority">
              @for (p of priorities; track p) { <option [value]="p">{{ p }}</option> }
            </select></div>
          <div class="form-field"><label class="label">Environment</label>
            <select class="select" formControlName="environment">
              @for (e of environments; track e) { <option [value]="e">{{ e }}</option> }
            </select></div>
          <div class="form-field"><label class="label">Component</label>
            <input class="input" formControlName="component" placeholder="PolicyEngine" /></div>
        </div>
        <div class="form-field"><label class="label">Steps to reproduce</label>
          <textarea class="textarea" formControlName="stepsToReproduce"></textarea></div>
        <div class="form-grid">
          <div class="form-field"><label class="label">Expected result</label>
            <textarea class="textarea" formControlName="expectedResult"></textarea></div>
          <div class="form-field"><label class="label">Actual result</label>
            <textarea class="textarea" formControlName="actualResult"></textarea></div>
        </div>
        <div class="form-grid">
          <div class="form-field"><label class="label">Trace ID</label>
            <input class="input mono" formControlName="traceId" /></div>
          <div class="form-field"><label class="label">Test Run ID</label>
            <input class="input mono" formControlName="testRunId" /></div>
        </div>
        <div style="display:flex;gap:10px">
          <button class="btn btn-primary" type="submit" [disabled]="busy()">{{ busy() ? 'Submitting…' : 'Submit defect' }}</button>
          <a class="btn btn-ghost" routerLink="/defects">Cancel</a>
        </div>
      </form>
    </div></div>
  `
})
export class DefectFormComponent implements OnInit {
  private readonly svc = inject(DefectsService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly severities = SEVERITIES;
  readonly priorities = DEFECT_PRIORITIES;
  readonly environments = ENVIRONMENTS;
  readonly busy = signal(false);
  readonly error = signal<string | null>(null);
  readonly fromAi = signal(false);

  readonly form = new FormGroup({
    title: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(5)] }),
    description: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    severity: new FormControl<Severity>('Medium', { nonNullable: true }),
    priority: new FormControl<DefectPriority>('Medium', { nonNullable: true }),
    environment: new FormControl('QA', { nonNullable: true }),
    component: new FormControl(''),
    stepsToReproduce: new FormControl(''),
    expectedResult: new FormControl(''),
    actualResult: new FormControl(''),
    traceId: new FormControl(''),
    testRunId: new FormControl(''),
    testCaseId: new FormControl('')
  });

  ngOnInit(): void {
    const qp = this.route.snapshot.queryParamMap;
    this.form.patchValue({
      testRunId: qp.get('testRunId') ?? '',
      testCaseId: qp.get('testCaseId') ?? '',
      traceId: qp.get('traceId') ?? ''
    });

    const draft = history.state?.['draft'] as Partial<CreateDefectRequest> | undefined;
    if (draft) {
      this.fromAi.set(true);
      this.form.patchValue({ ...draft } as Record<string, string>);
    }
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.busy.set(true);
    this.svc.create(this.form.getRawValue() as CreateDefectRequest)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: d => { this.toast.success('Defect reported.'); this.router.navigateByUrl(`/defects/${d.id}`); },
        error: (err: ApiError) => {
          this.busy.set(false);
          this.error.set(err.fieldErrors.join(' ') || err.detail || err.title);
        }
      });
  }
}