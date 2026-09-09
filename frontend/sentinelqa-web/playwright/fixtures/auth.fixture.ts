import { test as base } from '@playwright/test';
import { SeededUser, uiLogin } from '../helpers/auth';

type SentinelFixtures = {
  authenticatedPage: void;
  adminPage: void;
};

export const test = base.extend<SentinelFixtures & { loginAs: (user?: SeededUser) => Promise<void> }>({
  loginAs: async ({ page }, use) => {
    await use((user = 'admin') => uiLogin(page, user));
  },
  authenticatedPage: [async ({ page, loginAs }, use) => {
    await loginAs('qa');
    await use();
  }, { auto: false }],
  adminPage: [async ({ page, loginAs }, use) => {
    await loginAs('admin');
    await use();
  }, { auto: false }]
});

export { expect } from '@playwright/test';