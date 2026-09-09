import { Severity, DefectPriority } from './defect.models';

export interface AiTestScenario {
  name: string;
  type: string;
  priority: string;
  description: string;
  steps?: string[];
  expected?: string;
  rationale?: string;
}

export interface GenerateScenariosRequest {
  requirement: string;
  context?: string;
  maxScenarios?: number;
}

export interface GenerateScenariosResponse {
  scenarios: AiTestScenario[];
  rationale?: string;
  model?: string;
  confidence?: number;
}

export interface AnalyzeFailureRequest {
  expected?: string;
  actual?: string;
  errorMessage?: string;
  traceId?: string;
  logs?: string;
}

export interface FailureAnalysis {
  summary: string;
  likelyRootCause: string;
  relatedRules?: string[];
  suggestedReproduction?: string;
  suggestedSeverity?: string;
  confidence?: number;
  model?: string;
}

export interface DraftDefectRequest {
  testRunId?: string;
  testCaseId?: string;
  failureMessage?: string;
  traceId?: string;
}

export interface AiDraftDefect {
  title: string;
  description: string;
  severity: Severity;
  priority: DefectPriority;
  stepsToReproduce?: string;
  expectedResult?: string;
  actualResult?: string;
  confidence?: number;
}