export type NetworkType = 'Corporate' | 'DMZ' | 'Management' | 'Cloud' | 'Guest' | 'Other';
export const NETWORK_TYPES: NetworkType[] = ['Corporate', 'DMZ', 'Management', 'Cloud', 'Guest', 'Other'];

export interface Network {
  id: string;
  tenantId: string;
  name: string;
  cidr: string;
  type: NetworkType;
  description?: string;
  createdAt: string;
}

export interface CreateNetworkRequest {
  name: string;
  cidr: string;
  type: NetworkType;
  description?: string;
}

export interface NetworkGroup {
  id: string;
  name: string;
  description?: string;
  networkIds: string[];
}

export interface ServiceDefinition {
  id: string;
  name: string;
  protocol: 'TCP' | 'UDP' | 'ICMP' | 'Any';
  port: number;
  description?: string;
}