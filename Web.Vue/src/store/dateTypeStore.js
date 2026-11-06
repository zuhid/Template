import { clearStore, getAll, saveAll } from '../lib/db.js';

export class DateTypeStore {
  #apiClient;
  #url = "dateType";

  constructor(apiClient) {
    this.#apiClient = apiClient;
  }

  async getAll(options) {
    const cached = await getAll('dateTypes');
    if (!cached) {
      const data = await this.#apiClient.get(this.#url);
      if (data) await saveAll('dateTypes', data);
    }
    return await getAll('dateTypes', options);
  }

  async get(id) {
    const data = await this.#apiClient.get(`${this.#url}/${id}`);
    const rows = Array.isArray(data) ? data : data ? [data] : [];
    return rows.find((r) => String(r.id).toLowerCase() === String(id).toLowerCase()) ?? null;
  }
  create = async (model) => this.#apiClient.post(this.#url, model);
  update = async (model) => this.#apiClient.put(this.#url, model);
  delete = async (id) => this.#apiClient.delete(`${this.#url}/${id}`);

  async forceRefresh() {
    await clearStore('dateTypes');
    const data = await this.#apiClient.get(this.#url);
    if (data) await saveAll('dateTypes', data);
  }
}
