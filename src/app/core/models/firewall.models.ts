export type FirewallStatus = 'Active' | 'Inactive' | 'Maintenance' | 'Unreachable';

export const FIREWALL_VENDORS = [
  'Simulated', 'Cisco', 'Palo Alto', 'Fortinet', 'Check Point',
  'Azure Firewall', 'AWS Network Firewall', 'pfSense'
] as const;

export const ENVIRONMENTS = ['Development', 'QA', 'Staging', 'Production', 'DR'] as const;

export interface Firewall {
  id: string;
  tenantId: string;
  name: string;
  vendor: string;
  environment: string;
  status: FirewallStatus;
  ipAddress?: string;
  model?: string;
  firmwareVersion?: string;
  lastHealthCheckAt?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateFirewallRequest {
  name: string;
  vendor: string;
  environment: string;
  ipAddress?: string;
  model?: string;
}

export interface UpdateFirewallRequest extends CreateFirewallRequest {
  version?: number;
}