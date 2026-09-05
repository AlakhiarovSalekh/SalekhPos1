/**
 * Playwright smoke for the auth flow.
 *
 * The API is mocked at the network layer (page.route) so the
 * test runs in CI without a live backend. We walk:
 *   1. /login renders
 *   2. Negative login → invalid credentials banner
 *   3. Register page renders
 *   4. Successful login → /account shows user id
 *
 * Tagged @a11y so the existing `npm run test:a11y` script also
 * runs axe-core against every step.
 */
import { test, expect, type Page } from '@playwright/test';

function mockAuthApi(page: Page) {
  // Successful login returns a fake JWT with sub/tid/role/ver claims.
  const header = Buffer.from(JSON.stringify({ alg: 'EdDSA', typ: 'JWT' })).toString('base64url');
  const payload = Buffer.from(
    JSON.stringify({
      sub: '11111111-1111-1111-1111-111111111111',
      tid: '22222222-2222-2222-2222-222222222222',
      role: 'Owner',
      ver: 1,
      iss: 'salekhpos',
      aud: 'salekhpos.web',
      exp: Math.floor(Date.now() / 1000) + 900,
      iat: Math.floor(Date.now() / 1000),
    }),
  ).toString('base64url');
  const signature = 'fake-signature';
  const accessToken = `${header}.${payload}.${signature}`;
  const refreshToken = 'opaque-refresh-token';
  const expires = (offsetSeconds: number) =>
    new Date(Date.now() + offsetSeconds * 1000).toISOString();

  return page.route('**/api/v1/auth/**', async (route) => {
    const request = route.request();
    const url = new URL(request.url());
    const path = url.pathname;
    if (path.endsWith('/auth/login') && request.method() === 'POST') {
      const body = JSON.parse(request.postData() ?? '{}') as { email?: string; password?: string };
      if (body.email === 'bad@example.com') {
        await route.fulfill({
          status: 401,
          contentType: 'application/problem+json',
          body: JSON.stringify({
            type: 'https://salekhpos.com/errors/auth.invalid_credentials',
            title: 'Authentication failed',
            status: 401,
            code: 'auth.invalid_credentials',
            message: 'Invalid credentials.',
          }),
        });
        return;
      }
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          status: 'authenticated',
          tokens: {
            accessToken,
            refreshToken,
            accessTokenExpiresAtUtc: expires(900),
            refreshTokenExpiresAtUtc: expires(86_400),
          },
        }),
      });
      return;
    }
    if (path.endsWith('/auth/refresh') && request.method() === 'POST') {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          accessToken,
          refreshToken,
          accessTokenExpiresAtUtc: expires(900),
          refreshTokenExpiresAtUtc: expires(86_400),
        }),
      });
      return;
    }
    if (path.endsWith('/auth/logout') && request.method() === 'POST') {
      await route.fulfill({ status: 204, body: '' });
      return;
    }
    if (path.endsWith('/auth/register') && request.method() === 'POST') {
      await route.fulfill({
        status: 202,
        contentType: 'application/json',
        body: JSON.stringify({
          userId: '11111111-1111-1111-1111-111111111111',
          tenantId: '22222222-2222-2222-2222-222222222222',
          tenantSlug: 'acme',
        }),
      });
      return;
    }
    if (path.endsWith('/auth/forgot-password') && request.method() === 'POST') {
      await route.fulfill({ status: 204, body: '' });
      return;
    }
    if (path.endsWith('/auth/reset-password') && request.method() === 'POST') {
      await route.fulfill({ status: 204, body: '' });
      return;
    }
    if (path.endsWith('/auth/verify-email') && request.method() === 'POST') {
      await route.fulfill({ status: 204, body: '' });
      return;
    }
    if (path.endsWith('/auth/mfa/setup') && request.method() === 'POST') {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          provisioningUri: 'otpauth://totp/Test?secret=ABCDEFGH&issuer=Test',
          recoveryCodes: ['aaaa-bbbb-cccc', 'dddd-eeee-ffff'],
        }),
      });
      return;
    }
    if (path.endsWith('/auth/mfa/verify') && request.method() === 'POST') {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          accessToken,
          refreshToken,
          accessTokenExpiresAtUtc: expires(900),
          refreshTokenExpiresAtUtc: expires(86_400),
        }),
      });
      return;
    }
    // Fallback: pass through.
    await route.continue();
  });
}

test.describe('Auth flow @a11y', () => {
  test('login page renders the title, email and password fields', async ({ page }) => {
    await mockAuthApi(page);
    await page.goto('/login');
    await expect(page.getByRole('heading', { level: 2, name: /sign in/i })).toBeVisible();
    await expect(page.getByLabel(/email/i)).toBeVisible();
    await expect(page.getByLabel(/password/i)).toBeVisible();
    await expect(page.getByRole('button', { name: /sign in/i })).toBeVisible();
  });

  test('bad credentials show the invalid-credentials banner', async ({ page }) => {
    await mockAuthApi(page);
    await page.goto('/login');
    await page.getByLabel(/email/i).fill('bad@example.com');
    await page.getByLabel(/password/i).fill('WrongPassword-123!');
    await page.getByRole('button', { name: /sign in/i }).click();
    await expect(page.getByRole('alert')).toContainText(/invalid email or password/i);
  });

  test('successful login redirects to /account and shows the user id', async ({ page }) => {
    await mockAuthApi(page);
    await page.goto('/login');
    await page.getByLabel(/email/i).fill('owner@acme.test');
    await page.getByLabel(/password/i).fill('CorrectHorse-123!');
    await page.getByRole('button', { name: /sign in/i }).click();
    await page.waitForURL('**/account');
    await expect(page.getByText('11111111-1111-1111-1111-111111111111')).toBeVisible();
    await expect(page.getByRole('button', { name: /sign out/i })).toBeVisible();
  });

  test('register page renders the business fields', async ({ page }) => {
    await mockAuthApi(page);
    await page.goto('/register');
    await expect(page.getByRole('heading', { level: 2, name: /create your business account/i })).toBeVisible();
    await expect(page.getByLabel(/email/i)).toBeVisible();
    await expect(page.getByLabel(/business url/i)).toBeVisible();
  });

  test('forgot-password page renders the email field', async ({ page }) => {
    await mockAuthApi(page);
    await page.goto('/forgot-password');
    await expect(page.getByLabel(/email/i)).toBeVisible();
  });
});
