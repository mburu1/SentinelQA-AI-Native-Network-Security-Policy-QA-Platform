import { expect, test } from '../fixtures/auth.fixture';

test.describe('Policy workflow', () => {
  test('admin can create a policy with a valid rule', async ({ page, loginAs }) => {
    await loginAs('admin');

    await page.goto('/policies/new');
    await page.locator('input[formControlName="name"]').fill('e2e-regression-policy');
    await page.locator('select[formControlName="firewallId"]').selectOption({ index: 1 });

    const rule = page.locator('.rule-grid').first();
    await rule.locator('input[formControlName="sourceCidr"]').fill('10.0.0.0/16');
    await rule.locator('input[formControlName="destinationCidr"]').fill('192.168.1.0/24');
    await rule.locator('input[formControlName="port"]').fill('443');

    await page.getByRole('button', { name: /Create policy/ }).click();
    await page.waitForURL(/\/policies\/.+/);
    await expect(page.getByText('e2e-regression-policy')).toBeVisible();
  });

  test('client-side CIDR validation blocks invalid rule input', async ({ page, loginAs }) => {
    await loginAs('admin');
    await page.goto('/policies/new');

    const rule = page.locator('.rule-grid').first();
    await rule.locator('input[formControlName="sourceCidr"]').fill('10.0.0.0/33');
    await rule.locator('input[formControlName="sourceCidr"]').blur();

    await expect(page.getByText('Invalid CIDR').first()).toBeVisible();
    await expect(page.getByRole('button', { name: /Create policy/ })).toBeDisabled();
  });

  test('policy validation surfaces public SSH exposure as critical', async ({ page, loginAs }) => {
    await loginAs('admin');

    // Assumes seeded policy "seed-public-ssh-exposure" contains a dangerous rule.
    await page.goto('/policies');
    await page.getByText(/public-ssh/i).first().click();
    await page.getByTestId('validate-policy').click();

    await expect(page.getByText(/Public SSH exposure|CRITICAL/i).first()).toBeVisible({ timeout: 20_000 });
  });
});