import { expect } from '@playwright/test';

export class NumericTypesPage {
  constructor(page) {
    this.page = page;
    this.card = page.locator('#numeric_types');
    this.addButton = page.locator('#add_numeric_type');
    this.searchInput = page.getByRole('searchbox');
    this.addForm = page.locator('#add_numeric_type_form');
    this.editForm = page.locator('#edit_numeric_type_form');
    this.saveButton = page.getByRole('button', { name: 'Save' });
    this.cancelButton = page.getByRole('button', { name: 'Cancel' });
    this.numericTypesLink = page.getByRole('link', { name: 'Numeric Types' });
  }

  async goto() {
    await this.page.goto('/numeric-types');
  }

  async gotoNew() {
    await this.page.goto('/numeric-types/new');
  }

  async gotoEdit(id) {
    await this.page.goto(`/numeric-types/${id}/edit`);
  }

  async expectLoaded() {
    await this.expectList();
  }

  async expectList() {
    await expect(this.page).toHaveURL(/\/numeric-types$/);
    await expect(this.addButton).toBeVisible();
  }

  field(label) {
    return this.page.getByLabel(label, { exact: true });
  }

  // Validation message for a model key: smallintType, integerType or bigintType.
  error(key) {
    return this.page.getByTestId(`${key}_error`);
  }

  async fillForm({ smallintType, integerType, bigintType }) {
    if (smallintType !== undefined) await this.field('Smallint').fill(smallintType);
    if (integerType !== undefined) await this.field('Integer').fill(integerType);
    if (bigintType !== undefined) await this.field('Bigint').fill(bigintType);
  }

  async expectAddForm() {
    await expect(this.page).toHaveURL(/\/numeric-types\/new$/);
    await expect(this.addForm).toBeVisible();
  }

  async expectEditForm() {
    await expect(this.page).toHaveURL(/\/numeric-types\/[^/]+\/edit$/);
    await expect(this.editForm).toBeVisible();
  }

  async openAdd() {
    await this.addButton.click();
    await this.expectAddForm();
  }

  // Click Save without expecting navigation (used for validation failures).
  async trySave() {
    await this.saveButton.click();
  }

  async save() {
    await this.saveButton.click();
    await this.expectList();
  }

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

  async idFor(text) {
    const editId = await this.row(text).locator('[id^="edit_"]').getAttribute('id');
    return editId.slice('edit_'.length);
  }

  async edit(text) {
    await this.row(text).getByRole('button', { name: 'Edit' }).click();
    await this.expectEditForm();
  }

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
