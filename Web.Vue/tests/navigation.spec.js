import { test, expect } from './fixtures.js';

test.describe('routing', () => {
  test('unauthenticated root redirects to login', async ({ page }) => {
    await page.goto('/');
    await expect(page).toHaveURL(/\/login$/);
  });

  test('unauthenticated string-types redirects to login', async ({ page }) => {
    await page.goto('/string-types');
    await expect(page).toHaveURL(/\/login$/);
  });

  test('unknown route unauthenticated ends at login', async ({ page }) => {
    await page.goto('/does-not-exist');
    await expect(page).toHaveURL(/\/login$/);
  });

  test('unknown route authenticated redirects to string types', async ({ authedPage }) => {
    await authedPage.goto('/does-not-exist');
    await expect(authedPage).toHaveURL(/\/string-types$/);
  });

  test('navbar links switch between home and string types', async ({ authedPage, stringTypesPage }) => {
    await stringTypesPage.homeLink.click();
    await expect(authedPage).toHaveURL(/\/home$/);
    await stringTypesPage.stringTypesLink.click();
    await expect(authedPage).toHaveURL(/\/string-types$/);
  });

  for (const path of ['/string-types/new', '/string-types/1/edit', '/date-types/new', '/date-types/1/edit']) {
    test(`unauthenticated ${path} redirects to login`, async ({ page }) => {
      await page.goto(path);
      await expect(page).toHaveURL(/\/login$/);
    });
  }

  test('add form routes load directly when authenticated and show only the form', async ({ authedPage, stringTypesPage }) => {
    await authedPage.goto('/string-types/new');
    await expect(stringTypesPage.addForm).toBeVisible();
    await expect(stringTypesPage.addButton).toHaveCount(0);
    await authedPage.goto('/date-types/new');
    await expect(authedPage.locator('#add_date_type_form')).toBeVisible();
  });

  test('edit route with an unknown id redirects back to the list', async ({ authedPage }) => {
    await authedPage.goto('/string-types/00000000-0000-0000-0000-000000000000/edit');
    await expect(authedPage).toHaveURL(/\/string-types$/);
    await authedPage.goto('/date-types/00000000-0000-0000-0000-000000000000/edit');
    await expect(authedPage).toHaveURL(/\/date-types$/);
  });
});
