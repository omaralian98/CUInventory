import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:44353/',
  redirectUri: baseUrl,
  clientId: 'CUInventory_App',
  responseType: 'code',
  scope: 'offline_access CUInventory',
  requireHttps: true,
};

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'CUInventory',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://localhost:44353',
      rootNamespace: 'CUInventory',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
  remoteEnv: {
    url: '/getEnvConfig',
    mergeStrategy: 'deepmerge'
  }
} as Environment;
