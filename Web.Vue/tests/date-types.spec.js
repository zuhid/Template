import { test, expect } from './fixtures.js';
import { random } from './random.js';

// Row display: the two datetime columns render "YYYY-MM-DD HH:mm:ss" (null -> em dash); the others render raw API text.
const spaced = (v) => v.replace('T', ' ');

test.describe('date types', () => {
  test('navbar link opens the date types page', async ({ page, dateTypesPage }) => {
    await dateTypesPage.homeLink.click();
    await expect(page).toHaveURL(/\/home$/);
    await dateTypesPage.dateTypesLink.click();
    await dateTypesPage.expectLoaded();
    expect(page.url()).toMatch(/\/date-types$/);
    await expect(dateTypesPage.card).toBeVisible();
  });

  test('initial list request is authorized and succeeds', async ({ page, loginPage }) => {
    await loginPage.loginAs();
    const request = page.waitForRequest((r) => /\/dateType$/i.test(r.url()) && r.method() === 'GET');
    await page.goto('/date-types');
    const req = await request;
    expect(req.headers()['authorization']).toMatch(/^Bearer .+/);
    expect((await req.response()).status()).toBe(200);
  });

  test('unauthenticated date-types redirects to login', async ({ page }) => {
    await page.goto('/date-types');
    await expect(page).toHaveURL(/\/login$/);
  });

  test('add form opens empty and cancel closes it', async ({ dateTypesPage }) => {
    await dateTypesPage.openAdd();
    await dateTypesPage.expectForm({ dateTimeWithoutTzType: '', dateTimeType: '', timeSpanType: '', dateOnlyType: '', timeOnlyType: '' });
    await dateTypesPage.cancel();
  });

  test('cancel does not create a row', async ({ dateTypesPage }) => {
    const data = random.dateType();
    await dateTypesPage.openAdd();
    await dateTypesPage.fillForm(data);
    await dateTypesPage.cancel();
    await dateTypesPage.search(data.dateOnlyType);
    await expect(dateTypesPage.row(data.dateOnlyType)).toHaveCount(0);
  });

  test('add with values creates a row that displays the round-tripped values', async ({ dateTypesPage }) => {
    const data = random.dateType();
    await dateTypesPage.add(data);
    await dateTypesPage.search(data.dateOnlyType);
    const row = dateTypesPage.row(data.dateOnlyType);
    await expect(row).toHaveCount(1);
    await expect(row).toContainText(spaced(data.dateTimeWithoutTzType));
    // timestamptz is converted to the browser's local time for display, so it should match the input exactly.
    await expect(row).toContainText(spaced(data.dateTimeType));
    await expect(row).toContainText(data.timeSpanType);
    await expect(row).toContainText(data.timeOnlyType);
  });

  test('edit form shows the saved values and edit updates the row', async ({ dateTypesPage }) => {
    const data = random.dateType();
    await dateTypesPage.add(data);
    await dateTypesPage.search(data.dateOnlyType);
    const id = await dateTypesPage.idFor(data.dateOnlyType);
    await dateTypesPage.editById(id);
    await dateTypesPage.expectForm(data);

    const updated = random.dateType();
    await dateTypesPage.fillForm(updated);
    await dateTypesPage.save();

    await dateTypesPage.search(updated.dateOnlyType);
    const row = dateTypesPage.rowById(id);
    await expect(row).toHaveCount(1);
    await expect(row).toContainText(spaced(updated.dateTimeWithoutTzType));
    await expect(row).toContainText(spaced(updated.dateTimeType));
    await expect(row).toContainText(updated.timeSpanType);
    await expect(row).toContainText(updated.timeOnlyType);
    await dateTypesPage.search(data.dateOnlyType);
    await expect(dateTypesPage.rowById(id)).toHaveCount(0);
  });

  test('add with all fields empty saves a row of nulls', async ({ dateTypesPage }) => {
    // The list is paged (10/page), shared with parallel tests, and the POST response carries no id, so
    // diff the ids cached in IndexedDB (refreshed after every save) and take the new all-null row.
    await expect(dateTypesPage.addButton).toBeVisible();
    const before = await dateTypesPage.cachedIds({ allNull: true });
    const posted = dateTypesPage.page.waitForRequest((r) => /\/dateType$/i.test(r.url()) && r.method() === 'POST');
    await dateTypesPage.add({});
    expect(JSON.parse((await posted).postData())).toEqual({
      dateTimeWithoutTzType: null,
      dateTimeType: null,
      timeSpanType: null,
      dateOnlyType: null,
      timeOnlyType: null,
    });
    let id;
    await expect
      .poll(async () => {
        const fresh = [...(await dateTypesPage.cachedIds({ allNull: true }))].filter((c) => !before.has(c));
        return (id = fresh[0]);
      })
      .toBeTruthy();
    const created = [id];
    await dateTypesPage.revealRow(id);

    await expect(dateTypesPage.rowById(created[0]).getByText('—')).toHaveCount(5);

    // Edit form opens with every input empty (null round-trips as empty).
    await dateTypesPage.editById(created[0]);
    await dateTypesPage.expectForm({ dateTimeWithoutTzType: '', dateTimeType: '', timeSpanType: '', dateOnlyType: '', timeOnlyType: '' });
    await dateTypesPage.cancel();
    await dateTypesPage.revealRow(created[0]);

    await dateTypesPage.delete(created[0], { byId: true });
    await expect(dateTypesPage.rowById(created[0])).toHaveCount(0);
  });

  test('clearing a value on edit saves it as null', async ({ dateTypesPage }) => {
    const data = random.dateType();
    await dateTypesPage.add(data);
    await dateTypesPage.search(data.dateOnlyType);
    const id = await dateTypesPage.idFor(data.dateOnlyType);
    await dateTypesPage.editById(id);
    await dateTypesPage.fillForm({ dateTimeWithoutTzType: '', dateTimeType: '', timeSpanType: '', timeOnlyType: '' });
    await dateTypesPage.save();

    await dateTypesPage.search(data.dateOnlyType);
    const row = dateTypesPage.rowById(id);
    // Four cleared columns render as an em dash; the date column keeps its value.
    await expect(row.getByText('—')).toHaveCount(4);
    await expect(row).toContainText(data.dateOnlyType);
    await dateTypesPage.editById(id);
    await dateTypesPage.expectForm({ dateTimeWithoutTzType: '', dateTimeType: '', timeSpanType: '', timeOnlyType: '', dateOnlyType: data.dateOnlyType });
  });

  test('time span accepts a value just under 24 hours', async ({ dateTypesPage }) => {
    const data = { ...random.dateType(), timeSpanType: '23:59:59' };
    await dateTypesPage.add(data);
    await dateTypesPage.search(data.dateOnlyType);
    await expect(dateTypesPage.row(data.dateOnlyType)).toContainText('23:59:59');
  });

  test('minute-only time input is stored with seconds', async ({ dateTypesPage }) => {
    const data = { ...random.dateType(), timeOnlyType: '07:08' };
    await dateTypesPage.add(data);
    await dateTypesPage.search(data.dateOnlyType);
    await expect(dateTypesPage.row(data.dateOnlyType)).toContainText('07:08:00');
  });

  test('cancel on edit leaves the row unchanged', async ({ dateTypesPage }) => {
    const data = random.dateType();
    await dateTypesPage.add(data);
    await dateTypesPage.search(data.dateOnlyType);
    await dateTypesPage.edit(data.dateOnlyType);
    await dateTypesPage.field('dateOnlyType').fill('1000-01-01');
    await dateTypesPage.cancel();
    await dateTypesPage.search(data.dateOnlyType);
    await expect(dateTypesPage.row(data.dateOnlyType)).toHaveCount(1);
  });

  test('delete removes a row after confirming', async ({ dateTypesPage }) => {
    const data = random.dateType();
    await dateTypesPage.add(data);
    await dateTypesPage.search(data.dateOnlyType);
    const id = await dateTypesPage.idFor(data.dateOnlyType);
    const message = await dateTypesPage.delete(id, { byId: true });
    await expect(dateTypesPage.rowById(id)).toHaveCount(0);
    expect(message()).toContain(id);
  });

  test('dismissing the delete confirm keeps the row', async ({ dateTypesPage }) => {
    const data = random.dateType();
    await dateTypesPage.add(data);
    await dateTypesPage.search(data.dateOnlyType);
    const message = await dateTypesPage.delete(data.dateOnlyType, { accept: false });
    await expect.poll(() => message()).toBeTruthy();
    await expect(dateTypesPage.row(data.dateOnlyType)).toHaveCount(1);
  });

  test('search filters rows and shows an empty state', async ({ dateTypesPage }) => {
    const a = random.dateType();
    let b = random.dateType();
    while (b.dateOnlyType === a.dateOnlyType) b = random.dateType();
    await dateTypesPage.add(a);
    await dateTypesPage.add(b);

    await dateTypesPage.search(a.dateOnlyType);
    await expect(dateTypesPage.row(a.dateOnlyType)).toHaveCount(1);
    await expect(dateTypesPage.row(b.dateOnlyType)).toHaveCount(0);

    await dateTypesPage.search(random.value('nomatch'));
    await expect(dateTypesPage.page.getByText('No results.')).toBeVisible();
  });

  test('added row survives a reload', async ({ page, dateTypesPage }) => {
    const data = random.dateType();
    await dateTypesPage.add(data);
    await page.reload();
    await dateTypesPage.search(data.dateOnlyType);
    const row = dateTypesPage.row(data.dateOnlyType);
    await expect(row).toHaveCount(1);
    await expect(row).toContainText(spaced(data.dateTimeType));
  });
});
