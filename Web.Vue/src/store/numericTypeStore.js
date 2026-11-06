import { clearStore, getAll, saveAll } from '../lib/db.js';

export class NumericTypeStore {
  #apiClient;
  #url = "numericType";

  constructor(apiClient) {
    this.#apiClient = apiClient;
  }

  async getAll(options) {
    const cached = await getAll('numericTypes');
    if (!cached) {
      const data = await this.#apiClient.get(this.#url);
      if (data) await saveAll('numericTypes', data);
    }
    return await getAll('numericTypes', options);
  }

  async get(id) {
    const data = await this.#apiClient.get(`${this.#url}/${id}`);
    const rows = Array.isArray(data) ? data : data ? [data] : [];
    return rows.find((r) => String(r.id).toLowerCase() === String(id).toLowerCase()) ?? null;
  }
  // Body is pre-serialized so bigintType (BigInt) keeps full long precision.
  #json = (m) => `{${Object.entries(m).map(([k, v]) => `${JSON.stringify(k)}:${typeof v === 'bigint' ? v.toString() : JSON.stringify(v)}`).join(',')}}`;
  create = async (model) => this.#apiClient.post(this.#url, this.#json(model));
  update = async (model) => this.#apiClient.put(this.#url, this.#json(model));
  delete = async (id) => this.#apiClient.delete(`${this.#url}/${id}`);

  async forceRefresh() {
    await clearStore('numericTypes');
    const data = await this.#apiClient.get(this.#url);
    if (data) await saveAll('numericTypes', data);
  }
}
