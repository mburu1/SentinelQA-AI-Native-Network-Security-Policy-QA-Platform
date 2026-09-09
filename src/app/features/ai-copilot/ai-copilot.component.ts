import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { concat } from 'rxjs';
import { AiService } from '@core/services/ai.service';
import { TestSuitesService } from '@core/services/test-suites.service';
import { ToastService } from '@core/services/toast.service';
import {
  AiDraftDefect, AiTestScenario, AnalyzeFailureRequest, FailureAnalysis, TestSuite
} from '@core/models';
import { PageHeaderComponent } from '@shared/components/page-header.component';
import { SpinnerComponent } from '@shared/components/spinner.component';
import { StatusBadgeComponent } from '@shared/components/status-badge.component';

interface ScenarioVM extends AiTestScenario { selected: boolean; }
type Tab = 'scenarios' | 'failure' | 'defect';

@Component({
  selector: 'sq-ai-copilot',
  imports: [PageHeaderComponent, SpinnerComponent, StatusBadgeComponent],
  template: `
    <sq-page-header title="AI QA Copilot"
      subtitle="Local Ollama/Qwen — output is advisory and must be verified before use" />

    <div class="tabs">
      <button class="tab" [class.active]="tab() === 'scenarios'" (click)="tab.set('scenarios')">✨ Requirement → Scenarios</button>
      <button class="tab" [class.active]="tab() === 'failure'" (click)="tab.set('failure')">🔍 Failure Analysis</button>
      <button class="tab" [class.active]="tab() === 'defect'" (click)="tab.set('defect')">🐞 Defect Draft</button>
    </div>

    @if (error(); as err) { <div class="alert alert-error">{{ err }}</div> }

    @switch (tab()) {
      @case ('scenarios') {
        <div class="grid-2">
          <div class="card">
            <div class="card-head"><span class="card-title">Requirement</span></div>
            <div class="card-body">
              <textarea class="textarea" style="min-height:140px"
                placeholder="Users must not be able to expose SSH to the public internet."
                [value]="requirement()" (input)="requirement.set($any($event.target).value)"></textarea>
              <div style="margin-top:12px;display:flex;gap:10px;align-items:center">
                <button class="btn btn-primary" (click)="generate()" [disabled]="generating()">
                  {{ generating() ? 'Generating…' : '✨ Generate test scenarios' }}
                </button>
                @if (confidence(); as c) { <span class="chip">model confidence {{ (c * 100).toFixed(0) }}%</span> }
              </div>
              @if (generating()) { <div class="page-loading"><sq-spinner /></div> }
            </div>
          </div>
          <div class="card">
            <div class="card-head"><span class="card-title">Generated scenarios ({{ scenarios().length }})</span></div>
            <div class="card-body">
              @for (s of scenarios(); track s.name; let i = $index) {
                <div class="scenario-card">
                  <label style="display:flex;gap:10px;align-items:flex-start;cursor:pointer">
                    <input type="checkbox" [checked]="s.selected" (change)="toggle(i)" style="margin-top:4px" />
                    <span>
                      <strong>{{ s.name }}</strong>
                      <span style="margin-left:8px"><sq-status-badge [label]="s.type" /></span>
                      <span style="margin-left:6px"><sq-status-badge [label]="s.priority" /></span>
                      <div class="muted small" style="margin-top:4px">{{ s.description }}</div>
                      @if (s.rationale) { <div class="small" style="margin-top:4px">💡 {{ s.rationale }}</div> }
                    </span>
                  </label>
                </div>
              } @empty {
                <div class="muted">Generated scenarios will appear here.</div>
              }
              @if (scenarios().length) {
                <div style="display:flex;gap:10px;margin-top:12px;align-items:center">
                  <select class="select" style="max-width:280px" [value]="targetSuiteId()"
                          (change)="targetSuiteId.set($any($event.target).value)">
                    <option value="">Select suite to add to…</option>
                    @for (suite of suites(); track suite.id) { <option [value]="suite.id">{{ suite.name }}</option> }
                  </select>
                  <button class="btn btn-primary" (click)="addToSuite()" [disabled]="adding()">
                    {{ adding() ? 'Adding…' : 'Add selected to suite' }}
                  </button>
                </div>
              }
            </div>
          </div>
        </div>
      }

      @case ('failure') {
        <div class="grid-2">
          <div class="card">
            <div class="card-head"><span class="card-title">Failure evidence</span></div>
            <div class="card-body">
              <div class="form-grid">
                <div class="form-field"><label class="label">Expected</label>
                  <input class="input" [value]="failure().expected" (input)="patchFailure('expected', $event)" placeholder="201 Created" /></div>
                <div class="form-field"><label class="label">Actual</label>
                  <input class="input" [value]="failure().actual" (input)="patchFailure('actual', $event)" placeholder="409 Conflict" /></div>
              </div>
              <div class="form-field"><label class="label">Error message</label>
                <input class="input mono" [value]="failure().errorMessage" (input)="patchFailure('errorMessage', $event)" placeholder="PolicyConflictException" /></div>
              <div class="form-field"><label class="label">Trace ID</label>
                <input class="input mono" [value]="failure().traceId" (input)="patchFailure('traceId', $event)" /></div>
              <button class="btn btn-primary" (click)="analyze()" [disabled]="analyzing()">
                {{ analyzing() ? 'Analyzing…' : '🔍 Analyze failure' }}
              </button>
              @if (analyzing()) { <div class="page-loading"><sq-spinner /></div> }
            </div>
          </div>
          <div class="card">
            <div class="card-head"><span class="card-title">Root cause hypothesis</span></div>
            <div class="card-body">
              @if (analysis(); as a) {
                <div class="kv">
                  <span class="kv-key">Summary</span><span class="kv-val">{{ a.summary }}</span>
                  <span class="kv-key">Likely root cause</span><span class="kv-val">{{ a.likelyRootCause }}</span>
                  <span class="kv-key">Suggested severity</span><span class="kv-val">{{ a.suggestedSeverity ?? '—' }}</span>
                  <span class="kv-key">Confidence</span><span class="kv-val">{{ a.confidence != null ? (a.confidence * 100).toFixed(0) + '%' : '—' }}</span>
                </div>
                @if (a.suggestedReproduction) { <h4 style="margin-top:14px">Suggested reproduction</h4><pre class="code-block">{{ a.suggestedReproduction }}</pre> }
                <div class="alert alert-warn" style="margin-top:14px">⚠️ AI output is advisory — verify before acting.</div>
              } @else { <div class="muted">Analysis output will appear here.</div> }
            </div>
          </div>
        </div>
      }

      @case ('defect') {
        <div class="grid-2">
          <div class="card">
            <div class="card-head"><span class="card-title">Failure context</span></div>
            <div class="card-body">
              <div class="form-field"><label class="label">Failure message</label>
                <textarea class="textarea" [value]="draftInput().failureMessage"
                  (input)="patchDraftInput('failureMessage', $event)"></textarea></div>
              <div class="form-grid">
                <div class="form-field"><label class="label">Test run ID</label>
                  <input class="input mono" [value]="draftInput().testRunId" (input)="patchDraftInput('testRunId', $event)" /></div>
                <div class="form-field"><label class="label">Trace ID</label>
                  <input class="input mono" [value]="draftInput().traceId" (input)="patchDraftInput('traceId', $event)" /></div>
              </div>
              <button class="btn btn-primary" (click)="draft()" [disabled]="drafting()">
                {{ drafting() ? 'Drafting…' : '🐞 Draft defect' }}
              </button>
            </div>
          </div>
          <div class="card">
            <div class="card-head"><span class="card-title">Draft preview</span></div>
            <div class="card-body">
              @if (draftResult(); as d) {
                <h4>{{ d.title }}</h4>
                <p class="muted">{{ d.description }}</p>
                <div style="display:flex;gap:8px;margin-bottom:12px">
                  <sq-status-badge [label]="d.severity" />
                  <sq-status-badge [label]="d.priority" />
                </div>
                <button class="btn btn-primary" (click)="openDefectForm(d)">Open in defect form →</button>
              } @else { <div class="muted">Draft preview will appear here.</div> }
            </div>
          </div>
        </div>
      }
    }
  `
})
export class AiCopilotComponent implements OnInit {
  private readonly ai = inject(AiService);
  private readonly suites = inject(TestSuitesService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly tab = signal<Tab>('scenarios');
  readonly error = signal<string | null>(null);

  // Scenarios tab
  readonly requirement = signal('');
  readonly generating = signal(false);
  readonly adding = signal(false);
  readonly scenarios = signal<ScenarioVM[]>([]);
  readonly confidence = signal<number | null>(null);
  readonly targetSuiteId = signal('');

  // Failure tab
  readonly failure = signal<AnalyzeFailureRequest>({});
  readonly analyzing = signal(false);
  readonly analysis = signal<FailureAnalysis | null>(null);

  // Defect tab
  readonly draftInput = signal<{ failureMessage?: string; testRunId?: string; traceId?: string }>({});
  readonly drafting = signal(false);
  readonly draftResult = signal<AiDraftDefect | null>(null);

  ngOnInit(): void {
    this.suites.list({ pageSize: 100 }).pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: r => this.suites$.set(r.items) });
  }

  readonly suites$ = signal<TestSuite[]>([]);
  suites() { return this.suites$(); }

  toggle(index: number): void {
    this.scenarios.update(list => list.map((s, i) => (i === index ? { ...s, selected: !s.selected } : s)));
  }

  patchFailure(key: keyof AnalyzeFailureRequest, event: Event): void {
    this.failure.update(f => ({ ...f, [key]: (event.target as HTMLInputElement).value }));
  }

  patchDraftInput(key: 'failureMessage' | 'testRunId' | 'traceId', event: Event): void {
    this.draftInput.update(d => ({ ...d, [key]: (event.target as HTMLInputElement).value }));
  }

  generate(): void {
    const requirement = this.requirement().trim();
    if (!requirement) return;
    this.error.set(null);
    this.generating.set(true);
    this.ai.generateScenarios({ requirement }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: res => {
        this.scenarios.set(res.scenarios.map(s => ({ ...s, selected: true })));
        this.confidence.set(res.confidence ?? null);
        this.generating.set(false);
      },
      error: () => { this.generating.set(false); this.error.set('AI scenario generation failed. Is Ollama running?'); }
    });
  }

  analyze(): void {
    this.error.set(null);
    this.analyzing.set(true);
    this.ai.analyzeFailure(this.failure()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: a => { this.analysis.set(a); this.analyzing.set(false); },
      error: () => { this.analyzing.set(false); this.error.set('Failure analysis failed.'); }
    });
  }

  draft(): void {
    this.error.set(null);
    this.drafting.set(true);
    this.ai.draftDefect(this.draftInput()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: d => { this.draftResult.set(d); this.drafting.set(false); },
      error: () => { this.drafting.set(false); this.error.set('Defect drafting failed.'); }
    });
  }

  openDefectForm(draft: AiDraftDefect): void {
    this.router.navigate(['/defects/new'], { state: { draft } });
  }

  addToSuite(): void {
    const suiteId = this.targetSuiteId();
    const selected = this.scenarios().filter(s => s.selected);
    if (!suiteId || selected.length === 0) return;

    this.adding.set(true);
    const requests = selected.map(s =>
      this.suites.addCase({
        testSuiteId: suiteId,
        name: s.name,
        description: s.description,
        type: (['Positive', 'Negative', 'Boundary', 'Security', 'Regression', 'Smoke', 'Performance'].includes(s.type)
          ? s.type : 'Regression') as never,
        priority: (['Critical', 'High', 'Medium', 'Low'].includes(s.priority) ? s.priority : 'Medium') as never,
        steps: s.steps?.join('\n'),
        expected: s.expected,
        automated: false
      })
    );

    concat(...requests).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      complete: () => {
        this.adding.set(false);
        this.toast.success(`${selected.length} AI scenario(s) added to suite.`);
        this.scenarios.set([]);
      },
      error: () => { this.adding.set(false); this.toast.error('Some scenarios could not be added.'); }
    });
  }
}