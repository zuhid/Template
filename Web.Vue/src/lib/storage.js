export class Storage {
  get token() { return localStorage.getItem("token"); }
  set token(value) { value ? localStorage.setItem("token", value) : localStorage.removeItem("token"); }

  clear() {
    localStorage.clear();
    sessionStorage.clear();
  }
}

export const storage = new Storage();
