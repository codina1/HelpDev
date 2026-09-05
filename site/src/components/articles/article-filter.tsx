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
    <section className="border-t border-white/[0.07] pt-3.5">
      <p className="mb-2 text-[11.5px] font-bold tracking-wide text-[#94A3B8]">{title}</p>
      <div className="space-y-0.5">{children}</div>
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
    <label className="flex cursor-pointer items-center justify-between gap-2 py-1.5 text-[12.5px] text-[#CBD5E1] transition hover:text-white">
      <span className="inline-flex min-w-0 items-center gap-2.5">
        <input type="radio" name={name} checked={checked} onChange={onSelect} className="sr-only" />
        <span
          className={[
            "flex h-4 w-4 shrink-0 items-center justify-center rounded-full border transition",
            checked
              ? "border-[#7C3AED] bg-[#7C3AED]/20 shadow-[0_0_8px_rgba(124,58,237,0.4)]"
              : "border-white/[0.22] bg-white/[0.03]",
          ].join(" ")}
          aria-hidden
        >
          {checked ? <span className="h-1.5 w-1.5 rounded-full bg-[#A855F7]" /> : null}
        </span>
        <span className="truncate">{label}</span>
      </span>
      {count != null ? (
        <span className="shrink-0 rounded-md bg-white/[0.04] px-1.5 py-0.5 text-[10.5px] font-semibold text-[#64748B]">
          {count.toLocaleString("fa-IR")}
        </span>
      ) : null}
    </label>
  );
}

/** Compact glass filter sidebar — search · topics · level · sort dropdown. */
export function ArticleFilter({ value, onChange }: Props) {
  return (
    <aside
      className="h-fit rounded-[16px] border border-white/[0.08] bg-[#0f172a]/80 p-4 shadow-[0_8px_28px_rgba(2,6,23,0.35)] backdrop-blur-xl"
      dir="rtl"
    >
      <h2 className="text-[14px] font-extrabold text-white">فیلتر مقالات</h2>

      <div className="mt-3.5">
        <p className="mb-2 text-[11.5px] font-bold tracking-wide text-[#94A3B8]">جستجو در مقالات</p>
        <div className="relative">
          <span className="pointer-events-none absolute inset-y-0 left-2.5 flex items-center text-[#64748B]">
            <SearchIcon className="h-3.5 w-3.5" />
          </span>
          <input
            type="search"
            value={value.query}
            onChange={(event) => onChange({ ...value, query: event.target.value })}
            placeholder="جستجوی مقاله..."
            className="h-9 w-full rounded-[10px] border border-white/[0.1] bg-[#070b18]/80 px-2.5 pl-9 text-[12.5px] text-white outline-none transition placeholder:text-[#64748B] focus:border-[rgba(168,85,247,0.45)] focus:shadow-[0_0_10px_rgba(124,58,237,0.16)]"
          />
        </div>
      </div>

      <div className="mt-4 space-y-4">
        <FilterGroup title="موضوعات">
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
          {ARTICLE_LEVELS.filter((item) => item.id !== "all").map((item) => (
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
            className="h-9 w-full rounded-[10px] border border-white/[0.1] bg-[#070b18]/80 px-2.5 text-[12.5px] text-white outline-none transition focus:border-[rgba(168,85,247,0.45)]"
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
