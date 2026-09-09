export type TestCaseType = 'Positive' | 'Negative' | 'Boundary' | 'Security' | 'Regression' | 'Smoke' | 'Performance';
export type TestCasePriority = 'Critical' | 'High' | 'Medium' | 'Low';
export type TestRunStatus = 'Pending' | 'Queued' | 'Running' | 'Completed' | 'Failed' | 'Cancelled';
export type TestResultStatus = 'Passed' | 'Failed' | 'Skipped' | 'Error' | 'InProgress';

export const TEST_CASE_TYPES: TestCaseType[] = ['Positive', 'Negative', 'Boundary', 'Security', 'Regression', 'Smoke', 'Performance'];
export const TEST_CASE_PRIORITIES: TestCasePriority[] = ['Critical', 'High', 'Medium', 'Low'];

export interface TestSuite {
  id: string;
  name: string;
  description?: string;
  isRegression: boolean;
  environment?: string;
  caseCount?: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTestSuiteRequest {
  name: string;
  description?: string;
  isRegression: boolean;
  environment?: string;
}

export interface TestCase {
  id: string;
  testSuiteId: string;
  name: string;
  description?: string;
  type: TestCaseType;
  priority: TestCasePriority;
  automated: boolean;
  steps?: string;
  expected?: string;
}

export interface CreateTestCaseRequest {
  testSuiteId: string;
  name: string;
  description?: string;
  type: TestCaseType;
  priority: TestCasePriority;
  steps?: string;
  expected?: string;
  automated?: boolean;
}

export interface TestRun {
  id: string;
  testSuiteId: string;
  testSuiteName?: string;
  environment: string;
  status: TestRunStatus;
  totalCases: number;
  passed: number;
  failed: number;
  skipped: number;
  triggeredBy?: string;
  startedAt?: string;
  completedAt?: string;
}

export interface TestResult {
  id: string;
  testRunId: string;
  testCaseId: string;
  testCaseName?: string;
  status: TestResultStatus;
  durationMs?: number;
  failureMessage?: string;
  traceId?: string;
  artifactUrl?: string;
}

export interface StartTestRunRequest {
  testSuiteId: string;
  environment: string;
}

export function isTerminalRunStatus(status: TestRunStatus): boolean {
  return status === 'Completed' || status === 'Failed' || status === 'Cancelled';
}