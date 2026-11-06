import { test, expect } from './fixtures.js';
import { random } from './random.js';

const valid = { smallintType: '5', integerType: '10', bigintType: '20' };

test.describe('numeric types: validation (no API data needed)', () => {
  test('nav link opens the list', async ({ authedPage, numericTypesPage }) => {
    await authedPage.goto('/string-types');
    await numericTypesPage.numericTypesLink.click();
    await numericTypesPage.expectList();
  });

  test('add form opens empty and cancel returns to the list', async ({ numericTypesPage: p }) => {
    await p.openAdd();
    await expect(p.field('Smallint')).toHaveValue('');
    await expect(p.field('Integer')).toHaveValue('');
    await expect(p.field('Bigint')).toHaveValue('');
    await p.cancel();
  });

  test('direct navigation to /new shows the add form', async ({ numericTypesPage: p }) => {
    await p.gotoNew();
    await p.expectAddForm();
  });

  test('empty save shows required errors on all fields', async ({ numericTypesPage: p }) => {
    await p.openAdd();
    await p.trySave();
    await expect(p.error('smallintType')).toHaveText('Smallint is required');
    await expect(p.error('integerType')).toHaveText('Integer is required');
    await expect(p.error('bigintType')).toHaveText('Bigint is required');
    await p.expectAddForm();
  });

  test('empty save sends no request', async ({ page, numericTypesPage: p }) => {
    const posts = [];
    page.on('request', (r) => { if (r.method() === 'POST' && /numerictype/i.test(r.url())) posts.push(r.url()); });
    await p.openAdd();
    await p.trySave();
    await expect(p.error('smallintType')).toBeVisible();
    expect(posts).toHaveLength(0);
  });

  const invalid = [
    ['non-numeric', 'abc', 'must be a whole number'],
    ['decimal', '1.5', 'must be a whole number'],
    ['whitespace only', '   ', 'is required'],
  ];
  for (const [name, value, msg] of invalid) {
    test(`${name} value is rejected`, async ({ numericTypesPage: p }) => {
      await p.openAdd();
      await p.fillForm({ smallintType: value, integerType: value, bigintType: value });
      await p.trySave();
      await expect(p.error('smallintType')).toHaveText(`Smallint ${msg}`);
      await expect(p.error('integerType')).toHaveText(`Integer ${msg}`);
      await expect(p.error('bigintType')).toHaveText(`Bigint ${msg}`);
    });
  }

  test('out of range values show range errors', async ({ numericTypesPage: p }) => {
    await p.openAdd();
    await p.fillForm({ smallintType: '101', integerType: '2147483648', bigintType: '501' });
    await p.trySave();
    await expect(p.error('smallintType')).toHaveText('Smallint must be between 0 and 100');
    await expect(p.error('integerType')).toHaveText('Integer must be between 0 and 2147483647');
    await expect(p.error('bigintType')).toHaveText('Bigint must be between -9223372036854775808 and 500');
  });

  test('below-minimum values show range errors', async ({ numericTypesPage: p }) => {
    await p.openAdd();
    await p.fillForm({ smallintType: '-1', integerType: '-1', bigintType: '-9223372036854775809' });
    await p.trySave();
    await expect(p.error('smallintType')).toHaveText('Smallint must be between 0 and 100');
    await expect(p.error('integerType')).toHaveText('Integer must be between 0 and 2147483647');
    await expect(p.error('bigintType')).toHaveText('Bigint must be between -9223372036854775808 and 500');
  });

  test('boundary values pass validation', async ({ page, numericTypesPage: p }) => {
    // Block the API so only client-side validation is under test.
    await page.route(/:5103\/numerictype/i, (route) => route.abort());
    await p.openAdd();
    await p.fillForm({ smallintType: '100', integerType: '2147483647', bigintType: '500' });
    await p.trySave();
    await expect(p.error('smallintType')).toHaveCount(0);
    await expect(p.error('integerType')).toHaveCount(0);
    await expect(p.error('bigintType')).toHaveCount(0);
    await p.fillForm({ smallintType: '0', integerType: '0', bigintType: '-9223372036854775808' });
    await p.trySave();
    await expect(p.error('smallintType')).toHaveCount(0);
    await expect(p.error('integerType')).toHaveCount(0);
    await expect(p.error('bigintType')).toHaveCount(0);
  });

  test('fixing an invalid field clears its error on the next save', async ({ page, numericTypesPage: p }) => {
    await page.route(/:5103\/numerictype/i, (route) => route.abort());
    await p.openAdd();
    await p.fillForm({ ...valid, smallintType: '101' });
    await p.trySave();
    await expect(p.error('smallintType')).toBeVisible();
    await p.field('Smallint').fill('100');
    await p.trySave();
    await expect(p.error('smallintType')).toHaveCount(0);
  });
});

test.describe('numeric types: CRUD against the live API', () => {
  test('list loads from the API with the bearer token', async ({ page, loginPage }) => {
    const req = page.waitForRequest((r) => /\/numericType$/i.test(r.url()) && r.method() === 'GET');
    await loginPage.loginAs();
    await page.goto('/numeric-types');
    const request = await req;
    expect(request.headers()['authorization']).toMatch(/^Bearer .+/);
    expect((await request.response()).status()).toBe(200);
  });

  test('add creates a row', async ({ numericTypesPage: p }) => {
    const data = random.numericType();
    await p.add(data);
    await p.search(data.integerType);
    const row = p.row(data.integerType);
    await expect(row).toHaveCount(1);
    await expect(row).toContainText(data.smallintType);
    await expect(row).toContainText(data.bigintType);
  });

  test('add accepts boundary values', async ({ numericTypesPage: p }) => {
    const data = { smallintType: '100', integerType: '2147483647', bigintType: '500' };
    await p.add(data);
    await p.search(data.integerType);
    await expect(p.row(data.integerType).first()).toContainText('500');
  });

  test('cancel does not create a row', async ({ numericTypesPage: p }) => {
    const data = random.numericType();
    await p.openAdd();
    await p.fillForm(data);
    await p.cancel();
    await p.search(data.integerType);
    await expect(p.row(data.integerType)).toHaveCount(0);
  });

  test('edit navigates to /:id/edit prefilled and saving updates the row', async ({ numericTypesPage: p }) => {
    const data = random.numericType();
    await p.add(data);
    await p.search(data.integerType);
    const id = await p.idFor(data.integerType);
    await p.edit(data.integerType);
    await expect(p.page).toHaveURL(new RegExp(`/numeric-types/${id}/edit$`));
    await expect(p.field('Smallint')).toHaveValue(data.smallintType);
    await expect(p.field('Integer')).toHaveValue(data.integerType);
    await expect(p.field('Bigint')).toHaveValue(data.bigintType);

    const updated = random.numericType();
    await p.fillForm(updated);
    await p.save();
    await p.search(updated.integerType);
    await expect(p.row(updated.integerType)).toHaveCount(1);
    await p.search(data.integerType);
    await expect(p.row(data.integerType)).toHaveCount(0);
  });

  test('direct navigation to /:id/edit prefills the form', async ({ numericTypesPage: p }) => {
    const data = random.numericType();
    await p.add(data);
    await p.search(data.integerType);
    const id = await p.idFor(data.integerType);
    await p.gotoEdit(id);
    await p.expectEditForm();
    await expect(p.field('Integer')).toHaveValue(data.integerType);
  });

  test('edit with an unknown id redirects to the list', async ({ numericTypesPage: p }) => {
    await p.gotoEdit('00000000-0000-0000-0000-000000000000');
    await p.expectList();
  });

  test('invalid edit does not change the row', async ({ numericTypesPage: p }) => {
    const data = random.numericType();
    await p.add(data);
    await p.search(data.integerType);
    await p.edit(data.integerType);
    await p.field('Smallint').fill('999');
    await p.trySave();
    await expect(p.error('smallintType')).toHaveText('Smallint must be between 0 and 100');
    await p.expectEditForm();
  });

  test('cancel on edit leaves the row unchanged', async ({ numericTypesPage: p }) => {
    const data = random.numericType();
    await p.add(data);
    await p.search(data.integerType);
    await p.edit(data.integerType);
    await p.field('Integer').fill('7');
    await p.cancel();
    await p.search(data.integerType);
    await expect(p.row(data.integerType)).toHaveCount(1);
  });

  test('delete removes a row after confirming', async ({ numericTypesPage: p }) => {
    const data = random.numericType();
    await p.add(data);
    await p.search(data.integerType);
    const message = await p.delete(data.integerType);
    await expect(p.row(data.integerType)).toHaveCount(0);
    expect(message()).toBe(`Delete "${data.smallintType} / ${data.integerType} / ${data.bigintType}"?`);
  });

  test('dismissing the delete confirm keeps the row', async ({ numericTypesPage: p }) => {
    const data = random.numericType();
    await p.add(data);
    await p.search(data.integerType);
    const message = await p.delete(data.integerType, { accept: false });
    await expect.poll(() => message()).toBeTruthy();
    await expect(p.row(data.integerType)).toHaveCount(1);
  });

  test('search shows an empty state', async ({ numericTypesPage: p }) => {
    await p.search('no-such-number');
    await expect(p.page.getByText('No results.')).toBeVisible();
  });

  test('added row survives a reload', async ({ page, numericTypesPage: p }) => {
    const data = random.numericType();
    await p.add(data);
    await page.reload();
    await p.search(data.integerType);
    await expect(p.row(data.integerType)).toHaveCount(1);
  });
});
