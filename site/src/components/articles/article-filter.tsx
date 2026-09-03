"use client";

import type { ReactNode } from "react";
import {
  ARTICLE_LEVELS,
  ARTICLE_SIDEBAR_TOPICS,
  ARTICLE_SORT_OPTIONS,
  type ArticleCategoryId,
  type ArticleLevelId,
  type ArticleSortId,
} from "@/data/articles";

export type ArticlesFiltersState = {
  query: string;
  topic: ArticleCategoryId;
  level: ArticleLevelId;
  sort: ArticleSortId;
};

export const DEFAULT_ARTICLES_FILTERS: ArticlesFiltersState = {
  query: "",
  topic: "all",
  level: "all",
  sort: "newest",
};

type Props = {
  value: ArticlesFiltersState;
  onChange: (next: ArticlesFiltersState) => void;
};

function SearchIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <circle cx="11" cy="11" r="7" stroke="currentColor" strokeWidth="1.8" />
      <path d="m20 20-3.2-3.2" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  );
}

function FilterGroup({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="border-t border-white/[0.07] pt-5">
      <p className="mb-3 text-[12px] font-bold tracking-wide text-[#94A3B8]">{title}</p>
      <div className="space-y-1">{children}</div>
    </section>
  );
}

function RadioRow({
  name,
  label,
  checked,
  onSelect,
  count,
}: {
  name: string;
  label: string;
  checked: boolean;
  onSelect: () => void;
  count?: number;
}) {
  return (
    <label className="flex cursor-pointer items-center justify-between gap-2 py-2 text-[13px] text-[#CBD5E1] transition hover:text-white">
      <span className="inline-flex items-center gap-3">
        <input type="radio" name={name} checked={checked} onChange={onSelect} className="sr-only" />
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
        {label}
      </span>
      {count != null ? (
        <span className="rounded-md bg-white/[0.04] px-1.5 py-0.5 text-[11px] font-semibold text-[#64748B]">
          {count.toLocaleString("fa-IR")}
        </span>
      ) : null}
    </label>
  );
}

/** Glass filter sidebar — search · topics · level · sort. */
export function ArticleFilter({ value, onChange }: Props) {
  return (
    <aside
      className="rounded-[16px] border border-white/[0.08] bg-[#0f172a]/80 p-5 shadow-[0_8px_28px_rgba(2,6,23,0.35)] backdrop-blur-xl"
      dir="rtl"
    >
      <h2 className="text-[15px] font-extrabold text-white">فیلتر مقالات</h2>

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
            placeholder="جستجوی مقاله..."
            className="h-11 w-full rounded-[12px] border border-white/[0.1] bg-[#070b18]/80 px-3 pl-10 text-[13px] text-white outline-none transition placeholder:text-[#64748B] focus:border-[rgba(168,85,247,0.45)] focus:shadow-[0_0_12px_rgba(124,58,237,0.18)]"
          />
        </div>
      </div>

      <div className="mt-6 space-y-6">
        <FilterGroup title="موضوعات">
          <RadioRow
            name="articles-topic"
            label="همه"
            checked={value.topic === "all"}
            onSelect={() => onChange({ ...value, topic: "all" })}
          />
          {ARTICLE_SIDEBAR_TOPICS.map((item) => (
            <RadioRow
              key={item.id}
              name="articles-topic"
              label={item.label}
              count={item.count}
              checked={value.topic === item.id}
              onSelect={() => onChange({ ...value, topic: item.id })}
            />
          ))}
        </FilterGroup>

        <FilterGroup title="سطح مقاله">
          {ARTICLE_LEVELS.map((item) => (
            <RadioRow
              key={item.id}
              name="articles-level"
              label={item.label}
              checked={value.level === item.id}
              onSelect={() => onChange({ ...value, level: item.id })}
            />
          ))}
        </FilterGroup>

        <FilterGroup title="مرتب‌سازی">
          <select
            value={value.sort}
            onChange={(event) =>
              onChange({ ...value, sort: event.target.value as ArticleSortId })
            }
            className="h-11 w-full rounded-[12px] border border-white/[0.1] bg-[#070b18]/80 px-3 text-[13px] text-white outline-none transition focus:border-[rgba(168,85,247,0.45)] focus:shadow-[0_0_12px_rgba(124,58,237,0.18)]"
          >
            {ARTICLE_SORT_OPTIONS.map((option) => (
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
