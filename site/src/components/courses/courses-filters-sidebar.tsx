"use client";

import {
  COURSE_LEVEL_FILTERS,
  COURSE_PRICE_FILTERS,
  type CoursePriceFilter,
} from "@/data/courses";
import type { CourseLevel } from "@/types";

export type CoursesFiltersState = {
  query: string;
  level: "all" | CourseLevel;
  price: CoursePriceFilter;
  maxHours: number;
};

export const COURSES_MAX_HOURS = 450;

export const DEFAULT_COURSES_FILTERS: CoursesFiltersState = {
  query: "",
  level: "all",
  price: "all",
  maxHours: COURSES_MAX_HOURS,
};

type Props = {
  value: CoursesFiltersState;
  onChange: (next: CoursesFiltersState) => void;
  onReset: () => void;
};

function SearchIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <circle cx="11" cy="11" r="7" stroke="currentColor" strokeWidth="1.8" />
      <path d="m20 20-3.2-3.2" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  );
}

function ResetIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path d="M4.5 9A7.6 7.6 0 1 1 4.2 14" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
      <path d="M4 4.6V9.2h4.6" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}

function OptionRow({
  label,
  checked,
  onSelect,
  name,
}: {
  label: string;
  checked: boolean;
  onSelect: () => void;
  name: string;
}) {
  return (
    <label className="flex cursor-pointer items-center gap-2.5 py-2 text-[13px] text-[#CBD5E1] transition hover:text-white">
      <input
        type="radio"
        name={name}
        checked={checked}
        onChange={onSelect}
        className="sr-only"
      />
      <span
        className={[
          "flex h-4 w-4 shrink-0 items-center justify-center rounded-full border transition",
          checked ? "border-[#7C3AED] bg-[#7C3AED]/15" : "border-white/[0.18] bg-transparent",
        ].join(" ")}
        aria-hidden
      >
        {checked ? <span className="h-[7px] w-[7px] rounded-full bg-[#A855F7]" /> : null}
      </span>
      {label}
    </label>
  );
}

/** Filters card — search · level · type · duration · reset (reference sidebar). */
export function CoursesFiltersSidebar({ value, onChange, onReset }: Props) {
  return (
    <div
      className="rounded-[16px] border border-white/[0.07] bg-[#0B1120] p-5 shadow-[0_6px_20px_rgba(2,6,23,0.28)]"
      dir="rtl"
    >
      <h2 className="text-[14.5px] font-extrabold text-white">جستجو در دوره‌ها</h2>

      <div className="relative mt-3.5">
        <span className="pointer-events-none absolute inset-y-0 left-3 flex items-center text-[#64748B]">
          <SearchIcon className="h-4 w-4" />
        </span>
        <input
          type="search"
          value={value.query}
          onChange={(event) => onChange({ ...value, query: event.target.value })}
          placeholder="جستجو در عنوان دوره..."
          aria-label="جستجو در عنوان دوره"
          className="h-11 w-full rounded-xl border border-white/[0.09] bg-[#0F1626] pe-3.5 ps-10 text-[13px] text-white outline-none transition placeholder:text-[#64748B] focus:border-[rgba(168,85,247,0.45)]"
        />
      </div>

      <div className="mt-6">
        <h3 className="text-[13.5px] font-bold text-white">سطح دوره</h3>
        <div className="mt-2">
          {COURSE_LEVEL_FILTERS.map((item) => (
            <OptionRow
              key={item.id}
              name="course-level"
              label={item.label}
              checked={value.level === item.id}
              onSelect={() => onChange({ ...value, level: item.id })}
            />
          ))}
        </div>
      </div>

      <div className="mt-6">
        <h3 className="text-[13.5px] font-bold text-white">نوع دوره</h3>
        <div className="mt-2">
          {COURSE_PRICE_FILTERS.map((item) => (
            <OptionRow
              key={item.id}
              name="course-price"
              label={item.label}
              checked={value.price === item.id}
              onSelect={() => onChange({ ...value, price: item.id })}
            />
          ))}
        </div>
      </div>

      <div className="mt-6">
        <h3 className="text-[13.5px] font-bold text-white">مدت زمان</h3>
        <input
          type="range"
          min={0}
          max={COURSES_MAX_HOURS}
          step={10}
          value={value.maxHours}
          onChange={(event) => onChange({ ...value, maxHours: Number(event.target.value) })}
          aria-label="حداکثر مدت زمان دوره"
          className="courses-range mt-3 w-full"
          style={{ "--range-fill": `${(value.maxHours / COURSES_MAX_HOURS) * 100}%` } as React.CSSProperties}
        />
        <div className="mt-2.5 flex items-center justify-between text-[11.5px] font-semibold text-[#64748B]">
          <span>+۴۵۰ ساعت</span>
          <span>۰ ساعت</span>
        </div>
      </div>

      <button
        type="button"
        onClick={onReset}
        className="mt-6 inline-flex h-11 w-full items-center justify-center gap-2 rounded-xl border border-white/[0.1] bg-[#0F1626] text-[13px] font-bold text-[#CBD5E1] transition hover:border-[rgba(168,85,247,0.4)] hover:text-white"
      >
        <ResetIcon className="h-4 w-4 text-[#A78BFA]" />
        پاک کردن فیلترها
      </button>
    </div>
  );
}
