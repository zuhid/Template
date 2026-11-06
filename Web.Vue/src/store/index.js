import { apiClient, storage, toasterStore } from "../lib/index.js";
import { AccountStore } from "./accountStore.js";
import { StringTypeStore } from "./stringTypeStore.js";
import { DateTypeStore } from "./dateTypeStore.js";
import { NumericTypeStore } from "./numericTypeStore.js";

export const accountStore = new AccountStore(apiClient, storage);
export const stringTypeStore = new StringTypeStore(apiClient);
export const dateTypeStore = new DateTypeStore(apiClient);
export const numericTypeStore = new NumericTypeStore(apiClient);
export { storage, toasterStore };
