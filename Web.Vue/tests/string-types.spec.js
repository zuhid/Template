import { test, expect } from './fixtures.js';
import { random } from './random.js';

// The Char/Varchar/Text columns are all PostgreSQL "text" (see Template/Migrations), so values are
// stored and returned unpadded and can be asserted verbatim.

test.describe('string types', () => {
  test('list loads from the API with the bearer token', async ({ page, loginPage }) => {
    const authHeader = page.waitForRequest((r) => /\/stringType$/i.test(r.url()) && r.method() === 'GET');
    await loginPage.loginAs();
    const request = await authHeader;
    expect(request.headers()['authorization']).toMatch(/^Bearer .+/);
    const response = await request.response();
    expect(response.status()).toBe(200);
  });

  test('add form opens empty and cancel closes it', async ({ stringTypesPage }) => {
    await stringTypesPage.openAdd();
    await expect(stringTypesPage.field('Char')).toHaveValue('');
    await stringTypesPage.cancel();
  });

  test('cancel does not create a row', async ({ stringTypesPage }) => {
    const data = random.stringType();
    await stringTypesPage.openAdd();
    await stringTypesPage.fillForm(data);
    await stringTypesPage.cancel();
    await stringTypesPage.search(data.varcharType);
    await expect(stringTypesPage.row(data.varcharType)).toHaveCount(0);
  });

  test('inputs enforce a 50 character maxlength', async ({ stringTypesPage }) => {
    await stringTypesPage.openAdd();
    await stringTypesPage.field('Varchar').fill('x'.repeat(80));
    await expect(stringTypesPage.field('Varchar')).toHaveValue('x'.repeat(50));
  });

  test('add creates a row', async ({ stringTypesPage }) => {
    const data = random.stringType();
    await stringTypesPage.add(data);
    await stringTypesPage.search(data.varcharType);
    const row = stringTypesPage.row(data.varcharType);
    await expect(row).toHaveCount(1);
    await expect(row).toContainText(data.charType);
    await expect(row).toContainText(data.textType);
  });

  test('edit updates a row', async ({ stringTypesPage }) => {
    const data = random.stringType();
    await stringTypesPage.add(data);
    await stringTypesPage.search(data.varcharType);
    await stringTypesPage.edit(data.varcharType);

    await expect(stringTypesPage.field('Char')).toHaveValue(data.charType);
    await expect(stringTypesPage.field('Varchar')).toHaveValue(data.varcharType);
    await expect(stringTypesPage.field('Text')).toHaveValue(data.textType);

    const updated = random.stringType();
    await stringTypesPage.fillForm(updated);
    await stringTypesPage.save();

    await stringTypesPage.search(updated.varcharType);
    await expect(stringTypesPage.row(updated.varcharType)).toHaveCount(1);
    await expect(stringTypesPage.row(updated.varcharType)).toContainText(updated.textType);
    await stringTypesPage.search(data.varcharType);
    await expect(stringTypesPage.row(data.varcharType)).toHaveCount(0);
  });

  test('cancel on edit leaves the row unchanged', async ({ stringTypesPage }) => {
    const data = random.stringType();
    await stringTypesPage.add(data);
    await stringTypesPage.search(data.varcharType);
    await stringTypesPage.edit(data.varcharType);
    await stringTypesPage.field('Varchar').fill(random.value('never'));
    await stringTypesPage.cancel();
    await stringTypesPage.search(data.varcharType);
    await expect(stringTypesPage.row(data.varcharType)).toHaveCount(1);
  });

  test('delete removes a row after confirming', async ({ stringTypesPage }) => {
    const data = random.stringType();
    await stringTypesPage.add(data);
    await stringTypesPage.search(data.varcharType);
    await expect(stringTypesPage.row(data.varcharType)).toHaveCount(1);

    const message = await stringTypesPage.delete(data.varcharType);
    await expect(stringTypesPage.row(data.varcharType)).toHaveCount(0);
    expect(message()).toContain(data.varcharType);
  });

  test('dismissing the delete confirm keeps the row', async ({ stringTypesPage }) => {
    const data = random.stringType();
    await stringTypesPage.add(data);
    await stringTypesPage.search(data.varcharType);
    const message = await stringTypesPage.delete(data.varcharType, { accept: false });
    await expect.poll(() => message()).toBeTruthy();
    await expect(stringTypesPage.row(data.varcharType)).toHaveCount(1);
  });

  test('search filters rows and shows an empty state', async ({ stringTypesPage }) => {
    const a = random.stringType();
    const b = random.stringType();
    await stringTypesPage.add(a);
    await stringTypesPage.add(b);

    await stringTypesPage.search(a.varcharType);
    await expect(stringTypesPage.row(a.varcharType)).toHaveCount(1);
    await expect(stringTypesPage.row(b.varcharType)).toHaveCount(0);

    await stringTypesPage.search(random.value('nomatch'));
    await expect(stringTypesPage.page.getByText('No results.')).toBeVisible();
  });

  test('added row survives a reload', async ({ page, stringTypesPage }) => {
    const data = random.stringType();
    await stringTypesPage.add(data);
    await page.reload();
    await stringTypesPage.search(data.varcharType);
    await expect(stringTypesPage.row(data.varcharType)).toHaveCount(1);
  });
});
