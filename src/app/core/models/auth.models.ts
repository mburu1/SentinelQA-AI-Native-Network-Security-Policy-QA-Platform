export const Roles = {
  Admin: 'Admin',
  SecurityEngineer: 'SecurityEngineer',
  QAEngineer: 'QAEngineer',
  Developer: 'Developer',
  Approver: 'Approver',
  Viewer: 'Viewer'
} as const;

export type Role = (typeof Roles)[keyof typeof Roles];

export interface UserInfo {
  id: string;
  tenantId: string;
  email: string;
  displayName: string;
  roles: string[];
}

export interface LoginRequest { email: string; password: string; }
export interface RefreshRequest { refreshToken: string; }

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserInfo;
}