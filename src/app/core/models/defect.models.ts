export type DefectStatus = 'Open' | 'Triaged' | 'InProgress' | 'Fixed' | 'Retest' | 'Verified' | 'Closed';
export type Severity = 'Critical' | 'High' | 'Medium' | 'Low';
export type DefectPriority = 'Urgent' | 'High' | 'Medium' | 'Low';

export const DEFECT_STATUSES: DefectStatus[] = ['Open', 'Triaged', 'InProgress', 'Fixed', 'Retest', 'Verified', 'Closed'];
export const SEVERITIES: Severity[] = ['Critical', 'High', 'Medium', 'Low'];
export const DEFECT_PRIORITIES: DefectPriority[] = ['Urgent', 'High', 'Medium', 'Low'];

/** Allowed defect workflow transitions. */
export const DEFECT_TRANSITIONS: Record<DefectStatus, DefectStatus[]> = {
  Open: ['Triaged', 'Closed'],
  Triaged: ['InProgress'],
  InProgress: ['Fixed'],
  Fixed: ['Retest'],
  Retest: ['Verified', 'InProgress'],
  Verified: ['Closed'],
  Closed: []
};

export interface Defect {
  id: string;
  title: string;
  description: string;
  severity: Severity;
  priority: DefectPriority;
  status: DefectStatus;
  environment: string;
  component?: string;
  assigneeId?: string;
  assigneeName?: string;
  reportedBy?: string;
  stepsToReproduce?: string;
  expectedResult?: string;
  actualResult?: string;
  testRunId?: string;
  testCaseId?: string;
  traceId?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateDefectRequest {
  title: string;
  description: string;
  severity: Severity;
  priority: DefectPriority;
  environment: string;
  component?: string;
  stepsToReproduce?: string;
  expectedResult?: string;
  actualResult?: string;
  testRunId?: string;
  testCaseId?: string;
  traceId?: string;
  assigneeId?: string;
}