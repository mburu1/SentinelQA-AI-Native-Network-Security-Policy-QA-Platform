export type ChangeRequestState =
  | 'Draft' | 'Submitted' | 'Validating' | 'Testing' | 'AwaitingApproval'
  | 'Approved' | 'Rejected' | 'Deploying' | 'Verification'
  | 'Completed' | 'Failed' | 'RolledBack' | 'Cancelled';

export type ChangeRisk = 'Low' | 'Medium' | 'High' | 'Critical';
export const CHANGE_RISKS: ChangeRisk[] = ['Low', 'Medium', 'High', 'Critical'];

export interface Approval {
  approverId: string;
  approverName?: string;
  decision: 'Approved' | 'Rejected';
  comment?: string;
  decidedAt: string;
}

export interface DeploymentInfo {
  status: string;
  attempt?: number;
  deployedAt?: string;
  deployedBy?: string;
}

export interface ChangeTimelineEntry {
  state: string;
  at: string;
  by?: string;
  note?: string;
}

export interface ChangeRequest {
  id: string;
  policyId: string;
  policyName?: string;
  title: string;
  reason: string;
  risk: ChangeRisk;
  requestedBy: string;
  requestedByName?: string;
  state: ChangeRequestState;
  approvals: Approval[];
  deployment?: DeploymentInfo;
  timeline: ChangeTimelineEntry[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateChangeRequestRequest {
  policyId: string;
  title: string;
  reason: string;
  risk: ChangeRisk;
}