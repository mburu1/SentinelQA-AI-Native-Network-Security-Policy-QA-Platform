import { Page } from '@playwright/test';

export const USERS = {
  admin: { email: 'admin@sentinelqa.local', password: 'Admin123!' },
  qa: { email: 'qa@sentinelqa.local', password: 'Qa123!Pass' },
  approver: { email: 'approver@sentinelqa.local', password: 'Approver123!' }
} as const;

export type SeededUser = keyof typeof USERS;

/** Real UI login — exercises the login form, auth interceptor and guard. */
export async function uiLogin(page: Page, user: SeededUser = 'admin'): Promise<void> {
  await page.goto('/login');
  await page.getByTestId('login-email').fill(USERS[user].email);
  await page.getByTestId('login-password').fill(USERS[user].password);
  await page.getByTestId('login-submit').click();
  await page.waitForURL('**/dashboard', { timeout: 15_000 });
}