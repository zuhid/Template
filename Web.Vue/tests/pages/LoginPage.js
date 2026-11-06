import { expect } from '@playwright/test';

export class LoginPage {
  constructor(page) {
    this.page = page;
    this.card = page.locator('div#login');
    this.emailInput = page.getByLabel('Email');
    this.passwordInput = page.getByLabel('Password');
    this.loginButton = page.getByRole('button', { name: 'Login' });
    this.logoutButton = page.locator('#logout');
  }

  async goto() {
    await this.page.goto('/login');
  }

  async fill({ email, password }) {
    if (email !== undefined) await this.emailInput.fill(email);
    if (password !== undefined) await this.passwordInput.fill(password);
  }

  async login() {
    await this.loginButton.click();
  }

  // Full login flow; the API ignores credentials and always issues a token.
  async loginAs({ email = 'admin@test.com', password = 'password' } = {}) {
    await this.goto();
    await this.fill({ email, password });
    await this.login();
    await expect(this.page).toHaveURL(/\/string-types$/);
  }

  async logout() {
    await this.logoutButton.click();
  }

  async token() {
    return this.page.evaluate(() => localStorage.getItem('token'));
  }
}
