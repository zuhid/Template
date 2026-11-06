import { expect } from '@playwright/test';

export class StringTypesPage {
  constructor(page) {
    this.page = page;
    this.card = page.locator('#string_types');
    this.addButton = page.locator('#add_string_type');
    this.searchInput = page.getByRole('searchbox');
    this.addForm = page.locator('#add_string_type_form');
    this.editForm = page.locator('#edit_string_type_form');
    this.saveButton = page.getByRole('button', { name: 'Save' });
    this.cancelButton = page.getByRole('button', { name: 'Cancel' });
    this.homeLink = page.getByRole('link', { name: 'Home' });
    this.stringTypesLink = page.getByRole('link', { name: 'String Types' });
  }

  async goto() {
    await this.page.goto('/string-types');
  }

  async expectLoaded() {
    await expect(this.page).toHaveURL(/\/string-types$/);
    await expect(this.addButton).toBeVisible();
  }

  // Form fields (shared by the add and edit forms; only one is open at a time).
  field(label) {
    return this.page.getByLabel(label, { exact: true });
  }

  async fillForm({ charType, varcharType, textType }) {
    if (charType !== undefined) await this.field('Char').fill(charType);
    if (varcharType !== undefined) await this.field('Varchar').fill(varcharType);
    if (textType !== undefined) await this.field('Text').fill(textType);
  }

  async expectEditForm() {
    await expect(this.page).toHaveURL(/\/string-types\/[^/]+\/edit$/);
    await expect(this.editForm).toBeVisible();
  }

  async openAdd() {
    await this.addButton.click();
    await expect(this.page).toHaveURL(/\/string-types\/new$/);
    await expect(this.addForm).toBeVisible();
  }

  // The list is only valid on the list route; forms live on their own pages.
  async expectList() {
    await expect(this.page).toHaveURL(/\/string-types$/);
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

  // Row ids are only exposed via the buttons (id="edit_<id>"), so read it from the row.
  async idFor(text) {
    const editId = await this.row(text).locator('[id^="edit_"]').getAttribute('id');
    return editId.slice('edit_'.length);
  }

  async edit(text) {
    await this.row(text).getByRole('button', { name: 'Edit' }).click();
    await this.expectEditForm();
  }

  // Accepts (or dismisses) the window.confirm raised by Delete and returns the dialog message.
  async delete(text, { accept = true } = {}) {
    let message;
    this.page.once('dialog', async (dialog) => {
      message = dialog.message();
      if (accept) await dialog.accept();
      else await dialog.dismiss();
    });
    await this.row(text).getByRole('button', { name: 'Delete' }).click();
    return () => message;
  }
}
