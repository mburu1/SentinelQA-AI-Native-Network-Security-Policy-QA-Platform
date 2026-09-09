export type PolicyStatus = 'Draft' | 'Active' | 'UnderReview' | 'Deprecated' | 'Archived';
export type RuleAction = 'Allow' | 'Deny';
export type Protocol = 'TCP' | 'UDP' | 'ICMP' | 'Any';
export type Direction = 'Inbound' | 'Outbound' | 'Any';
export type FindingSeverity = 'Critical' | 'High' | 'Medium' | 'Low' | 'Info';

export const PROTOCOLS: Protocol[] = ['TCP', 'UDP', 'ICMP', 'Any'];
export const RULE_ACTIONS: RuleAction[] = ['Allow', 'Deny'];
export const DIRECTIONS: Direction[] = ['Inbound', 'Outbound', 'Any'];

export interface PolicyRule {
  id: string;
  priority: number;
  sourceCidr: string;
  destinationCidr: string;
  protocol: Protocol;
  port: number;
  action: RuleAction;
  direction: Direction;
  loggingEnabled: boolean;
  description?: string;
}

export interface PolicyRuleDraft {
  priority: number;
  sourceCidr: string;
  destinationCidr: string;
  protocol: Protocol;
  port: number;
  action: RuleAction;
  direction: Direction;
  loggingEnabled: boolean;
  description?: string;
}

export interface Policy {
  id: string;
  firewallId: string;
  firewallName?: string;
  name: string;
  description?: string;
  environment: string;
  version: number;
  status: PolicyStatus;
  rules: PolicyRule[];
  createdBy?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePolicyRequest {
  firewallId: string;
  name: string;
  description?: string;
  environment: string;
  rules: PolicyRuleDraft[];
}

export interface UpdatePolicyRequest extends CreatePolicyRequest {
  version: number;
}

export interface PolicyFinding {
  code: string;
  severity: FindingSeverity;
  title: string;
  message: string;
  rulePriority?: number;
  recommendation?: string;
}

export interface PolicyValidationResult {
  policyId: string;
  isValid: boolean;
  findings: PolicyFinding[];
  analyzedAt: string;
}

export interface PolicyAnalysis {
  policyId: string;
  isValid: boolean;
  findings: PolicyFinding[];
  statistics: {
    totalRules: number;
    allowRules: number;
    denyRules: number;
    shadowedRules: number;
    conflictingRules: number;
    duplicateRules: number;
  };
  analyzedAt: string;
}