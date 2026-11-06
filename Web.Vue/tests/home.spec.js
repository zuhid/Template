import { test, expect } from './fixtures.js';

test.describe('home', () => {
  test('home page renders after login', async ({ authedPage, stringTypesPage }) => {
    await stringTypesPage.homeLink.click();
    await expect(authedPage).toHaveURL(/\/home$/);
    await expect(authedPage.getByRole('heading', { name: 'Home' })).toBeVisible();
  });

  test('unauthenticated /home redirects to login', async ({ page }) => {
    await page.goto('/home');
    await expect(page).toHaveURL(/\/login$/);
  });
});
