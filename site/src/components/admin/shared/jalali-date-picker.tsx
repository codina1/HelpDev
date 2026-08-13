"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import {
  JALALI_MONTHS,
  JALALI_WEEKDAYS,
  dateToJalali,
  formatJalaliDateTime,
  jalaliMonthLength,
  jalaliToDate,
  jalaliWeekdayIndex,
  pad2,
  parseLocalDateTime,
  toFaDigits,
  toLocalDateTimeValue,
  type JalaliDate,
} from "@/lib/admin/datetime/jalali";

type JalaliDateTimePickerProps = {
  id?: string;
  value: string;
  onChange: (value: string) => void;
  disabled?: boolean;
  invalid?: boolean;
};

/**
 * Persian (Jalali) date + time picker. Stores the same local `YYYY-MM-DDTHH:mm`
 * value the news form already sends to the API.
 */
export function JalaliDateTimePicker({
  id,
  value,
  onChange,
  disabled = false,
  invalid = false,
}: JalaliDateTimePickerProps) {
  const rootRef = useRef<HTMLDivElement>(null);
  const [open, setOpen] = useState(false);
  const selected = parseLocalDateTime(value);
  const selectedJalali = selected ? dateToJalali(selected) : dateToJalali(new Date());
  const [view, setView] = useState<JalaliDate>(selectedJalali);

  useEffect(() => {
    if (open) setView(selectedJalali);
    // Only sync the visible month when the popover opens.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open]);

  useEffect(() => {
    if (!open) return;
    const onPointerDown = (event: PointerEvent) => {
      if (!rootRef.current?.contains(event.target as Node)) setOpen(false);
    };
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") setOpen(false);
    };
    document.addEventListener("pointerdown", onPointerDown);
    document.addEventListener("keydown", onKeyDown);
    return () => {
      document.removeEventListener("pointerdown", onPointerDown);
      document.removeEventListener("keydown", onKeyDown);
    };
  }, [open]);

  const hours = selected?.getHours() ?? 12;
  const minutes = selected?.getMinutes() ?? 0;

  const cells = useMemo(() => {
    const offset = jalaliWeekdayIndex(view.year, view.month, 1);
    const length = jalaliMonthLength(view.year, view.month);
    const slots: Array<number | null> = [];
    for (let i = 0; i < offset; i += 1) slots.push(null);
    for (let day = 1; day <= length; day += 1) slots.push(day);
    return slots;
  }, [view.year, view.month]);

  const shiftMonth = (delta: number) => {
    let month = view.month + delta;
    let year = view.year;
    if (month < 1) {
      month = 12;
      year -= 1;
    } else if (month > 12) {
      month = 1;
      year += 1;
    }
    setView({ year, month, day: 1 });
  };

  const pickDay = (day: number) => {
    const next = jalaliToDate({ year: view.year, month: view.month, day }, hours, minutes);
    onChange(toLocalDateTimeValue(next));
  };

  const pickTime = (nextHours: number, nextMinutes: number) => {
    const base = selectedJalali;
    const next = jalaliToDate(base, nextHours, nextMinutes);
    onChange(toLocalDateTimeValue(next));
  };

  const label = formatJalaliDateTime(value) || "انتخاب تاریخ";

  return (
    <div ref={rootRef} className="relative">
      <button
        id={id}
        type="button"
        disabled={disabled}
        aria-haspopup="dialog"
        aria-expanded={open}
        aria-invalid={invalid}
        onClick={() => setOpen((current) => !current)}
        className="adm-input flex w-full items-center justify-between gap-2 text-start"
      >
        <span className={value ? "adm-text" : "adm-subtle"}>{label}</span>
        <AdminIcon name="calendar" size={16} />
      </button>

      {open ? (
        <div
          role="dialog"
          aria-label="تقویم شمسی"
          className="adm-surface absolute z-30 mt-1.5 w-full min-w-[260px] rounded-xl border border-[var(--adm-border)] p-3 shadow-lg"
        >
          <div className="mb-2 flex items-center justify-between gap-2">
            <button
              type="button"
              className="adm-btn adm-btn-ghost adm-focus px-2 py-1 text-[12px]"
              onClick={() => shiftMonth(-1)}
              aria-label="ماه قبل"
            >
              ›
            </button>
            <p className="adm-text text-[13px] font-semibold">
              {JALALI_MONTHS[view.month - 1]} {toFaDigits(view.year)}
            </p>
            <button
              type="button"
              className="adm-btn adm-btn-ghost adm-focus px-2 py-1 text-[12px]"
              onClick={() => shiftMonth(1)}
              aria-label="ماه بعد"
            >
              ‹
            </button>
          </div>

          <div className="grid grid-cols-7 gap-1 text-center">
            {JALALI_WEEKDAYS.map((day) => (
              <span key={day} className="adm-subtle py-1 text-[11px] font-semibold">
                {day}
              </span>
            ))}
            {cells.map((day, index) => {
              if (day == null) {
                return <span key={`empty-${index}`} />;
              }
              const isSelected =
                selectedJalali.year === view.year &&
                selectedJalali.month === view.month &&
                selectedJalali.day === day;
              return (
                <button
                  key={day}
                  type="button"
                  onClick={() => pickDay(day)}
                  className={`rounded-lg py-1.5 text-[12px] ${
                    isSelected
                      ? "bg-[var(--adm-accent)] font-bold text-[var(--adm-accent-fg)]"
                      : "adm-text hover:bg-[var(--adm-surface-2)]"
                  }`}
                >
                  {toFaDigits(day)}
                </button>
              );
            })}
          </div>

          <div className="mt-3 flex items-center gap-2 border-t border-[var(--adm-border)] pt-3">
            <label className="adm-subtle flex flex-1 items-center gap-1.5 text-[11px]">
              ساعت
              <select
                className="adm-input py-1 text-[12px]"
                value={hours}
                onChange={(event) => pickTime(Number(event.target.value), minutes)}
              >
                {Array.from({ length: 24 }, (_, hour) => (
                  <option key={hour} value={hour}>
                    {toFaDigits(pad2(hour))}
                  </option>
                ))}
              </select>
            </label>
            <label className="adm-subtle flex flex-1 items-center gap-1.5 text-[11px]">
              دقیقه
              <select
                className="adm-input py-1 text-[12px]"
                value={minutes}
                onChange={(event) => pickTime(hours, Number(event.target.value))}
              >
                {Array.from({ length: 60 }, (_, minute) => (
                  <option key={minute} value={minute}>
                    {toFaDigits(pad2(minute))}
                  </option>
                ))}
              </select>
            </label>
          </div>
        </div>
      ) : null}
    </div>
  );
}
