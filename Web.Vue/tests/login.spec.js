import { test, expect } from './fixtures.js';
import { random } from './random.js';

test.describe('login', () => {
  test('login form renders', async ({ loginPage }) => {
    await loginPage.goto();
    await expect(loginPage.card).toBeVisible();
    await expect(loginPage.emailInput).toBeVisible();
    await expect(loginPage.passwordInput).toBeVisible();
    await expect(loginPage.loginButton).toBeVisible();
  });

  test('successful login stores a token and lands on string types', async ({ page, loginPage }) => {
    await loginPage.goto();
    await loginPage.fill({ email: random.email(), password: random.value('pw') });
    await loginPage.login();
    await expect(page).toHaveURL(/\/string-types$/);
    expect(await loginPage.token()).toBeTruthy();
  });

  test('logout clears the token and returns to login', async ({ page, loginPage, authedPage }) => {
    expect(await loginPage.token()).toBeTruthy();
    await loginPage.logout();
    await expect(page).toHaveURL(/\/login$/);
    expect(await loginPage.token()).toBeNull();
  });

  test('after logout protected routes redirect to login', async ({ page, loginPage, authedPage }) => {
    await loginPage.logout();
    await expect(page).toHaveURL(/\/login$/);
    await page.goto('/string-types');
    await expect(page).toHaveURL(/\/login$/);
  });

  test('failed login stays on login page and stores no token', async ({ page, loginPage }) => {
    await page.route('**/login', (route) => route.request().method() === 'POST'
      ? route.fulfill({ status: 401, contentType: 'application/json', body: '{}' })
      : route.continue());
    await loginPage.goto();
    await loginPage.login();
    await expect(page).toHaveURL(/\/login$/);
    expect(await loginPage.token()).toBeNull();
  });
});
