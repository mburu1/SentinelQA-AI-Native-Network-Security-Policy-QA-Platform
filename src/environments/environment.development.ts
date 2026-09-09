import { Environment } from './environment';

export const environment: Environment = {
  production: false,
  // Requests are proxied to https://localhost:7043 via proxy.conf.json
  apiUrl: '/api/v1',
  environmentName: 'local'
};