import { describe, expect, it } from "vitest";
import {
  dateToJalali,
  formatJalaliDateTime,
  gregorianToJalali,
  jalaliMonthLength,
  jalaliToDate,
  jalaliToGregorian,
  parseLocalDateTime,
  toLocalDateTimeValue,
} from "./jalali";

describe("jalali conversion", () => {
  it("maps Nowruz dates both ways", () => {
    expect(gregorianToJalali(2024, 3, 20)).toEqual({ year: 1403, month: 1, day: 1 });
    expect(gregorianToJalali(2025, 3, 21)).toEqual({ year: 1404, month: 1, day: 1 });
    expect(gregorianToJalali(2026, 3, 21)).toEqual({ year: 1405, month: 1, day: 1 });
    expect(jalaliToGregorian(1405, 1, 1)).toEqual({ gy: 2026, gm: 3, gd: 21 });
  });

  it("maps 13 August 2026 to 22 Mordad 1405", () => {
    expect(gregorianToJalali(2026, 8, 13)).toEqual({ year: 1405, month: 5, day: 22 });
  });

  it("keeps Esfand length at 29 or 30", () => {
    expect(jalaliMonthLength(1403, 12)).toBeGreaterThanOrEqual(29);
    expect(jalaliMonthLength(1403, 12)).toBeLessThanOrEqual(30);
    expect(jalaliMonthLength(1405, 1)).toBe(31);
    expect(jalaliMonthLength(1405, 7)).toBe(30);
  });

  it("round-trips a local datetime through Jalali", () => {
    const parsed = parseLocalDateTime("2026-08-13T13:38");
    expect(parsed).not.toBeNull();
    const jalali = dateToJalali(parsed!);
    const back = jalaliToDate(jalali, 13, 38);
    expect(toLocalDateTimeValue(back)).toBe("2026-08-13T13:38");
  });

  it("formats the picker label in Persian", () => {
    expect(formatJalaliDateTime("2026-08-13T13:38")).toContain("مرداد");
    expect(formatJalaliDateTime("2026-08-13T13:38")).toContain("۱۴۰۵");
  });
});
