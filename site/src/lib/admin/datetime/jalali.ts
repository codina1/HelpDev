/** Gregorian ↔ Jalali conversion and local datetime helpers. No external calendar library. */

export type JalaliDate = {
  year: number;
  month: number;
  day: number;
};

export const JALALI_MONTHS = [
  "فروردین",
  "اردیبهشت",
  "خرداد",
  "تیر",
  "مرداد",
  "شهریور",
  "مهر",
  "آبان",
  "آذر",
  "دی",
  "بهمن",
  "اسفند",
] as const;

/** Saturday-first week used in Iran. */
export const JALALI_WEEKDAYS = ["ش", "ی", "د", "س", "چ", "پ", "ج"] as const;

function div(a: number, b: number): number {
  return ~~(a / b);
}

export function gregorianToJalali(gy: number, gm: number, gd: number): JalaliDate {
  const g_d_m = [0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334];
  const gy2 = gm > 2 ? gy + 1 : gy;
  let days =
    355666 +
    365 * gy +
    div(gy2 + 3, 4) -
    div(gy2 + 99, 100) +
    div(gy2 + 399, 400) +
    gd +
    g_d_m[gm - 1];
  let jy = -1595 + 33 * div(days, 12053);
  days %= 12053;
  jy += 4 * div(days, 1461);
  days %= 1461;
  if (days > 365) {
    jy += div(days - 1, 365);
    days = (days - 1) % 365;
  }
  const jm = days < 186 ? 1 + div(days, 31) : 7 + div(days - 186, 30);
  const jd = 1 + (days < 186 ? days % 31 : (days - 186) % 30);
  return { year: jy, month: jm, day: jd };
}

export function jalaliToGregorian(jy: number, jm: number, jd: number): {
  gy: number;
  gm: number;
  gd: number;
} {
  jy += 1595;
  let days =
    -355668 +
    365 * jy +
    div(jy, 33) * 8 +
    div((jy % 33) + 3, 4) +
    jd +
    (jm < 7 ? (jm - 1) * 31 : (jm - 7) * 30 + 186);
  let gy = 400 * div(days, 146097);
  days %= 146097;
  if (days > 36524) {
    gy += 100 * div(--days, 36524);
    days %= 36524;
    if (days >= 365) days += 1;
  }
  gy += 4 * div(days, 1461);
  days %= 1461;
  if (days > 365) {
    gy += div(days - 1, 365);
    days = (days - 1) % 365;
  }
  let gd = days + 1;
  const leap = (gy % 4 === 0 && gy % 100 !== 0) || gy % 400 === 0;
  const sal_a = [0, 31, leap ? 29 : 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
  let gm = 0;
  for (gm = 1; gm <= 12 && gd > sal_a[gm]; gm += 1) {
    gd -= sal_a[gm];
  }
  return { gy, gm, gd };
}

export function isJalaliLeap(year: number): boolean {
  const g = jalaliToGregorian(year, 12, 30);
  const j = gregorianToJalali(g.gy, g.gm, g.gd);
  return j.year === year && j.month === 12 && j.day === 30;
}

export function jalaliMonthLength(year: number, month: number): number {
  if (month <= 6) return 31;
  if (month <= 11) return 30;
  return isJalaliLeap(year) ? 30 : 29;
}

/** Saturday = 0 … Friday = 6 */
export function jalaliWeekdayIndex(year: number, month: number, day: number): number {
  const { gy, gm, gd } = jalaliToGregorian(year, month, day);
  const jsDay = new Date(gy, gm - 1, gd).getDay();
  return (jsDay + 1) % 7;
}

const DIGIT_FA = ["۰", "۱", "۲", "۳", "۴", "۵", "۶", "۷", "۸", "۹"];

export function toFaDigits(value: string | number): string {
  return String(value).replace(/\d/g, (digit) => DIGIT_FA[Number(digit)]);
}

export function pad2(value: number): string {
  return String(value).padStart(2, "0");
}

/** Parses `YYYY-MM-DDTHH:mm` as a local civil datetime. */
export function parseLocalDateTime(value: string): Date | null {
  const match = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2})/.exec(value.trim());
  if (!match) return null;
  const date = new Date(
    Number(match[1]),
    Number(match[2]) - 1,
    Number(match[3]),
    Number(match[4]),
    Number(match[5]),
  );
  return Number.isNaN(date.getTime()) ? null : date;
}

export function toLocalDateTimeValue(date: Date): string {
  return `${date.getFullYear()}-${pad2(date.getMonth() + 1)}-${pad2(date.getDate())}T${pad2(date.getHours())}:${pad2(date.getMinutes())}`;
}

export function dateToJalali(date: Date): JalaliDate {
  return gregorianToJalali(date.getFullYear(), date.getMonth() + 1, date.getDate());
}

export function jalaliToDate(jalali: JalaliDate, hours: number, minutes: number): Date {
  const { gy, gm, gd } = jalaliToGregorian(jalali.year, jalali.month, jalali.day);
  return new Date(gy, gm - 1, gd, hours, minutes);
}

export function formatJalaliDateTime(value: string): string {
  const date = parseLocalDateTime(value);
  if (!date) return "";
  const jalali = dateToJalali(date);
  const month = JALALI_MONTHS[jalali.month - 1];
  return `${toFaDigits(jalali.day)} ${month} ${toFaDigits(jalali.year)}، ساعت ${toFaDigits(pad2(date.getHours()))}:${toFaDigits(pad2(date.getMinutes()))}`;
}
