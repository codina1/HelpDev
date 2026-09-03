"use client";

import type { ReactNode } from "react";
import {
  PROMPT_LAB_LEVELS,
  PROMPT_LAB_SIDEBAR_CATEGORIES,
  PROMPT_LAB_SIDEBAR_MODELS,
  PROMPT_LAB_SORT_OPTIONS,
  type PromptLabLevelId,
  type PromptLabSortId,
} from "@/data/prompt-lab";

export type PromptLabFiltersState = {
  query: string;
  categories: string[];
  models: string[];
  level: PromptLabLevelId;
  sort: PromptLabSortId;
};

export const DEFAULT_PROMPT_LAB_FILTERS: PromptLabFiltersState = {
  query: "",
  categories: [],
  models: [],
  level: "all",
  sort: "newest",
};

type Props = {
  value: PromptLabFiltersState;
  onChange: (next: PromptLabFiltersState) => void;
};

function SearchIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <circle cx="11" cy="11" r="7" stroke="currentColor" strokeWidth="1.8" />
      <path d="m20 20-3.2-3.2" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  );
}

function CheckRow({
  label,
  checked,
  onToggle,
  count,
}: {
  label: string;
  checked: boolean;
  onToggle: () => void;
  count?: number;
}) {
  return (
    <label className="flex cursor-pointer items-center justify-between gap-2 py-2 text-[13px] text-[#CBD5E1] transition hover:text-white">
      <span className="inline-flex items-center gap-3">
        <input type="checkbox" checked={checked} onChange={onToggle} className="sr-only" />
        <span
          className={[
            "flex h-[18px] w-[18px] shrink-0 items-center justify-center rounded-[5px] border transition",
            checked
              ? "border-[#7C3AED] bg-[#7C3AED] shadow-[0_0_10px_rgba(124,58,237,0.45)]"
              : "border-white/[0.22] bg-white/[0.03]",
          ].join(" ")}
          aria-hidden
        >
          {checked ? (
            <svg className="h-2.5 w-2.5 text-white" viewBox="0 0 24 24" fill="none">
              <path d="M5 12.5 10 17l9-10" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round" />
            </svg>
          ) : null}
        </span>
        {label}
      </span>
      {count != null ? (
        <span className="rounded-md bg-white/[0.04] px-1.5 py-0.5 text-[11px] font-semibold text-[#64748B]">
          {count}
        </span>
      ) : null}
    </label>
  );
}

function FilterGroup({
  title,
  children,
}: {
  title: string;
  children: ReactNode;
}) {
  return (
    <section className="border-t border-white/[0.07] pt-5">
      <p className="mb-3 text-[12px] font-bold tracking-wide text-[#94A3B8]">{title}</p>
      <div className="space-y-1">{children}</div>
    </section>
  );
}

function toggleInList(list: string[], id: string): string[] {
  return list.includes(id) ? list.filter((item) => item !== id) : [...list, id];
}

/** Dark glass filter card — search · categories · models · level · sort. */
export function PromptLabFiltersSidebar({ value, onChange }: Props) {
  return (
    <aside
      className="rounded-[16px] border border-white/[0.08] bg-[#0f172a]/80 p-5 shadow-[0_8px_28px_rgba(2,6,23,0.35)] backdrop-blur-xl"
      dir="rtl"
    >
      <h2 className="text-[15px] font-extrabold text-white">فیلترها</h2>

      <div className="mt-5">
        <p className="mb-3 text-[12px] font-bold tracking-wide text-[#94A3B8]">جستجو</p>
        <div className="relative">
          <span className="pointer-events-none absolute inset-y-0 left-3 flex items-center text-[#64748B]">
            <SearchIcon className="h-4 w-4" />
          </span>
          <input
            type="search"
            value={value.query}
            onChange={(event) => onChange({ ...value, query: event.target.value })}
            placeholder="جستجوی پرامپت…"
            className="h-11 w-full rounded-[12px] border border-white/[0.1] bg-[#070b18]/80 px-3 pl-10 text-[13px] text-white outline-none transition placeholder:text-[#64748B] focus:border-[rgba(168,85,247,0.45)] focus:shadow-[0_0_12px_rgba(124,58,237,0.18)]"
          />
        </div>
      </div>

      <div className="mt-6 space-y-6">
        <FilterGroup title="دسته‌بندی‌ها">
          {PROMPT_LAB_SIDEBAR_CATEGORIES.map((item) => (
            <CheckRow
              key={item.id}
              label={item.label}
              count={item.count}
              checked={value.categories.includes(item.slug)}
              onToggle={() =>
                onChange({ ...value, categories: toggleInList(value.categories, item.slug) })
              }
            />
          ))}
        </FilterGroup>

        <FilterGroup title="مدل هوش مصنوعی">
          {PROMPT_LAB_SIDEBAR_MODELS.map((item) => (
            <CheckRow
              key={item.id}
              label={item.label}
              checked={value.models.includes(item.slug)}
              onToggle={() =>
                onChange({ ...value, models: toggleInList(value.models, item.slug) })
              }
            />
          ))}
        </FilterGroup>

        <FilterGroup title="سطح">
          {PROMPT_LAB_LEVELS.map((item) => {
            const checked = value.level === item.id;
            return (
              <label
                key={item.id}
                className="flex cursor-pointer items-center gap-3 py-2 text-[13px] text-[#CBD5E1] transition hover:text-white"
              >
                <input
                  type="radio"
                  name="prompt-lab-level"
                  checked={checked}
                  onChange={() => onChange({ ...value, level: item.id })}
                  className="sr-only"
                />
                <span
                  className={[
                    "flex h-[18px] w-[18px] shrink-0 items-center justify-center rounded-full border transition",
                    checked
                      ? "border-[#7C3AED] bg-[#7C3AED]/20 shadow-[0_0_10px_rgba(124,58,237,0.4)]"
                      : "border-white/[0.22] bg-white/[0.03]",
                  ].join(" ")}
                  aria-hidden
                >
                  {checked ? <span className="h-2 w-2 rounded-full bg-[#A855F7]" /> : null}
                </span>
                {item.label}
              </label>
            );
          })}
        </FilterGroup>

        <FilterGroup title="مرتب‌سازی">
          <select
            value={value.sort}
            onChange={(event) =>
              onChange({ ...value, sort: event.target.value as PromptLabSortId })
            }
            className="h-11 w-full rounded-[12px] border border-white/[0.1] bg-[#070b18]/80 px-3 text-[13px] text-white outline-none transition focus:border-[rgba(168,85,247,0.45)] focus:shadow-[0_0_12px_rgba(124,58,237,0.18)]"
          >
            {PROMPT_LAB_SORT_OPTIONS.map((option) => (
              <option key={option.id} value={option.id}>
                {option.label}
              </option>
            ))}
          </select>
        </FilterGroup>
      </div>
    </aside>
  );
}
