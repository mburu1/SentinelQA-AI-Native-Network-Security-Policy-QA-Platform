export interface Environment {
  production: boolean;
  apiUrl: string;
  environmentName: string;
}

export const environment: Environment = {
  production: true,
  apiUrl: '/api/v1',
  environmentName: 'production'
};