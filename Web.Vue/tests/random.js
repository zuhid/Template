let counter = 0;

function suffix() {
  counter += 1;
  return `${Date.now().toString(36)}${counter}${Math.floor(Math.random() * 1296).toString(36)}`;
}

// All values stay well under the 50 character maxlength of the Text component.
export const random = {
  // Unique, short, alphanumeric token, e.g. "k3j2h1a9".
  token() {
    return suffix();
  },

  // Unique value with a readable prefix, e.g. "char-k3j2h1a9".
  value(prefix = 'val') {
    return `${prefix}-${suffix()}`;
  },

  stringType() {
    return { charType: this.value('c'), varcharType: this.value('v'), textType: this.value('t') };
  },

  // Numeric values as strings. integerType is the row key (large range keeps collisions unlikely).
  numericType() {
    return {
      smallintType: String(Math.floor(Math.random() * 101)),
      integerType: String(1000 + Math.floor(Math.random() * 2147000000)),
      bigintType: String(-Math.floor(Math.random() * 1000000000)),
    };
  },

  // Unique-ish date values in input format. The date (year 1000-9999 plus month/day) is the row key.
  dateType() {
    const pad = (n) => String(n).padStart(2, '0');
    const year = 1000 + Math.floor(Math.random() * 9000);
    const month = pad(1 + Math.floor(Math.random() * 12));
    const day = pad(1 + Math.floor(Math.random() * 28));
    // Hours 01-22 and seconds 01-59: Playwright's fill() rejects datetime-local values with zero seconds (e.g. T00:00:00) with "Malformed value".
    const time = () => `${pad(1 + Math.floor(Math.random() * 22))}:${pad(Math.floor(Math.random() * 60))}:${pad(1 + Math.floor(Math.random() * 59))}`;
    return {
      dateTimeWithoutTzType: `${year}-${month}-${day}T${time()}`,
      dateTimeType: `${year}-${month}-${day}T${time()}`,
      timeSpanType: time(),
      dateOnlyType: `${year}-${month}-${day}`,
      timeOnlyType: time(),
    };
  },

  email() {
    return `user${suffix()}@example.com`;
  },
};
