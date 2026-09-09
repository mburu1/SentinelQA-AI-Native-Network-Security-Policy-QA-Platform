import { expect, test } from '@playwright/test';
import { USERS } from '../helpers/auth';

test.describe('Authentication', () => {
  test('unauthenticated user is redirected to /login', async ({ page }) => {
    await page.goto('/dashboard');
    await expect(page).toHaveURL(/\/login/);
  });

  test('shows validation error for invalid credentials', async ({ page }) => {
    await page.goto('/login');
    await page.getByTestId('login-email').fill('hacker@nowhere.local');
    await page.getByTestId('login-password').fill('wrong-password');
    await page.getByTestId('login-submit').click();
    await expect(page.getByTestId('login-error')).toBeVisible();
  });

  test('QA engineer can sign in and lands on dashboard', async ({ page }) => {
    await page.goto('/login');
    await page.getByTestId('login-email').fill(USERS.qa.email);
    await page.getByTestId('login-password').fill(USERS.qa.password);
    await page.getByTestId('login-submit').click();
    await page.waitForURL('**/dashboard');
    await expect(page.getByText('Security posture and quality engineering')).toBeVisible();
  });

  test('sign out returns to login page', async ({ page }) => {
    await page.goto('/login');
    await page.getByTestId('login-email').fill(USERS.admin.email);
    await page.getByTestId('login-password').fill(USERS.admin.password);
    await page.getByTestId('login-submit').click();
    await page.waitForURL('**/dashboard');
    await page.getByText('Administration', { exact: false }).first().waitFor({ state: 'detached' }).catch(() => undefined);
    await page.locator('.user-menu').click();
    await page.getByTestId('logout').click();
    await expect(page).toHaveURL(/\/login/);
  });
});