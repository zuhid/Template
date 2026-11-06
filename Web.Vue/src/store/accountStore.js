export class AccountStore {
  #apiClient;
  #storage;
  #url = "login";

  constructor(apiClient, storage) {
    this.#apiClient = apiClient;
    this.#storage = storage;
  }

  // The API currently ignores the credentials and issues a token for a hard-coded user.
  async login() {
    const response = await this.#apiClient.post(this.#url, {});
    if (response?.token) {
      this.#storage.token = response.token;
      return true;
    }
    return false;
  }

  logout() {
    this.#storage.clear();
  }
}
