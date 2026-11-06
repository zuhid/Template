import { expect, test as base } from '@playwright/test';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';
import { LoginPage } from './pages/LoginPage.js';
import { DateTypesPage } from './pages/DateTypesPage.js';
import { NumericTypesPage } from './pages/NumericTypesPage.js';
import { StringTypesPage } from './pages/StringTypesPage.js';

const coverageDir = path.join(path.dirname(fileURLToPath(import.meta.url)), '..', '.nyc_output');

export const test = base.extend({
  // With COVERAGE=true, dump the istanbul counters from the page after each test for nyc to merge.
  page: async ({ page }, use, testInfo) => {
    await use(page);

    if (!process.env.COVERAGE) return;
    const coverage = await page.evaluate(() => window.__coverage__ ?? null).catch(() => null);
    if (!coverage) return;

    fs.mkdirSync(coverageDir, { recursive: true });
    const safeName = testInfo.title.replace(/[^a-z0-9]/gi, '_').toLowerCase().slice(0, 60);
    fs.writeFileSync(path.join(coverageDir, `${safeName}_w${testInfo.workerIndex}_${Date.now()}.json`), JSON.stringify(coverage));
  },

  loginPage: async ({ page }, use) => use(new LoginPage(page)),

  // A page that is already logged in (real POST /login against the API) and sitting on /string-types.
  authedPage: async ({ page, loginPage }, use) => {
    await loginPage.loginAs();
    await use(page);
  },

  stringTypesPage: async ({ authedPage }, use) => {
    const stringTypes = new StringTypesPage(authedPage);
    await stringTypes.expectLoaded();
    await use(stringTypes);
  },

  numericTypesPage: async ({ authedPage }, use) => {
    const numericTypes = new NumericTypesPage(authedPage);
    await numericTypes.goto();
    await numericTypes.expectLoaded();
    await use(numericTypes);
  },

  dateTypesPage: async ({ authedPage }, use) => {
    const dateTypes = new DateTypesPage(authedPage);
    await dateTypes.goto();
    await dateTypes.expectLoaded();
    await use(dateTypes);
  },
});

export { expect };
