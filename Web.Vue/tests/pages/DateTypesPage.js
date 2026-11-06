import { expect } from '@playwright/test';

// Input ids (the DateTime component uses the id prop for both the input and its label).
const FIELD_IDS = {
  dateTimeWithoutTzType: 'date_time_without_tz_type',
  dateTimeType: 'date_time_type',
  timeSpanType: 'time_span_type',
  dateOnlyType: 'date_only_type',
  timeOnlyType: 'time_only_type',
};

export class DateTypesPage {
  constructor(page) {
    this.page = page;
    this.card = page.locator('#date_types');
    this.addButton = page.locator('#add_date_type');
    this.searchInput = page.getByRole('searchbox');
    this.addForm = page.locator('#add_date_type_form');
    this.editForm = page.locator('#edit_date_type_form');
    this.saveButton = page.getByRole('button', { name: 'Save' });
    this.cancelButton = page.getByRole('button', { name: 'Cancel' });
    this.dateTypesLink = page.getByRole('link', { name: 'Date Types' });
    this.homeLink = page.getByRole('link', { name: 'Home' });
  }

  async goto() {
    await this.page.goto('/date-types');
  }

  async expectLoaded() {
    await expect(this.page).toHaveURL(/\/date-types$/);
    await expect(this.addButton).toBeVisible();
  }

  field(key) {
    return this.page.locator(`#${FIELD_IDS[key]}`);
  }

  async fillForm(values) {
    for (const key of Object.keys(FIELD_IDS)) {
      if (values[key] !== undefined) await this.field(key).fill(values[key]);
    }
  }

  async expectForm(values) {
    for (const key of Object.keys(FIELD_IDS)) {
      if (values[key] !== undefined) await expect(this.field(key)).toHaveValue(values[key]);
    }
  }

  async expectEditForm() {
    await expect(this.page).toHaveURL(/\/date-types\/[^/]+\/edit$/);
    await expect(this.editForm).toBeVisible();
  }

  async openAdd() {
    await this.addButton.click();
    await expect(this.page).toHaveURL(/\/date-types\/new$/);
    await expect(this.addForm).toBeVisible();
  }

  // The list is only valid on the list route; forms live on their own pages.
  async expectList() {
    await expect(this.page).toHaveURL(/\/date-types$/);
    await expect(this.addButton).toBeVisible();
  }

  // Save navigates back to the list.
  async save() {
    await this.saveButton.click();
    await this.expectList();
  }

  // Cancel navigates back to the list.
  async cancel() {
    await this.cancelButton.click();
    await this.expectList();
  }

  async add(values) {
    await this.openAdd();
    await this.fillForm(values);
    await this.save();
  }

  async search(text) {
    await this.searchInput.fill(text);
  }

  row(text) {
    return this.page.locator('tbody tr').filter({ hasText: text });
  }

  rowById(id) {
    return this.page.locator('tbody tr').filter({ has: this.page.locator(`#edit_${id}`) });
  }

  async idFor(text) {
    const editId = await this.row(text).locator('[id^="edit_"]').getAttribute('id');
    return editId.slice('edit_'.length);
  }

  // Ids cached in the app's IndexedDB store (all pages), optionally only rows whose five values are all null.
  async cachedIds({ allNull = false } = {}) {
    return new Set(
      await this.page.evaluate(
        (onlyNull) =>
          new Promise((resolve, reject) => {
            const open = indexedDB.open('template');
            open.onerror = () => reject(open.error);
            open.onsuccess = () => {
              const req = open.result.transaction('dateTypes', 'readonly').objectStore('dateTypes').getAll();
              req.onerror = () => reject(req.error);
              req.onsuccess = () => {
                const keys = ['dateTimeWithoutTzType', 'dateTimeType', 'timeSpanType', 'dateOnlyType', 'timeOnlyType'];
                resolve(req.result.filter((r) => !onlyNull || keys.every((k) => r[k] == null)).map((r) => r.id));
              };
            };
          }),
        allNull,
      ),
    );
  }

  async rowIds() {
    const ids = await this.page.locator('tbody [id^="edit_"]').evaluateAll((els) => els.map((e) => e.id.slice(5)));
    return new Set(ids);
  }

  // The table is paged (10 rows); step through the pages until the row with this id is shown.
  async revealRow(id) {
    const next = this.page.getByRole('button', { name: '›' });
    const target = this.page.locator(`#edit_${id}`);
    await expect(this.addButton).toBeVisible();
    for (let i = 0; i < 100; i++) {
      if (await target.count()) return;
      if (!(await next.isEnabled().catch(() => false))) break;
      await next.click();
    }
    await expect(target).toBeVisible();
  }

  async edit(text) {
    await this.row(text).getByRole('button', { name: 'Edit' }).click();
    await this.expectEditForm();
  }

  async editById(id) {
    await this.page.locator(`#edit_${id}`).click();
    await this.expectEditForm();
  }

  // Accepts (or dismisses) the window.confirm raised by Delete and returns the dialog message.
  async delete(idOrText, { accept = true, byId = false } = {}) {
    let message;
    this.page.once('dialog', async (dialog) => {
      message = dialog.message();
      if (accept) await dialog.accept();
      else await dialog.dismiss();
    });
    const button = byId ? this.page.locator(`#delete_${idOrText}`) : this.row(idOrText).getByRole('button', { name: 'Delete' });
    await button.click();
    return () => message;
  }
}
