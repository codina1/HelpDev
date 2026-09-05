"use client";

import type { ReactNode } from "react";
import {
  ARTICLE_LEVELS,
  ARTICLE_SIDEBAR_TOPICS,
  ARTICLE_SORT_OPTIONS,
  type ArticleCategoryId,
  type ArticleLevelId,
  type ArticleSortId,
  type MarketplaceArticle,
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
  articles?: MarketplaceArticle[];
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
    <section className="border-t border-white/[0.07] pt-2">
      <p className="mb-1 text-[11px] font-bold tracking-wide text-[#94A3B8]">{title}</p>
      <div className="space-y-0">{children}</div>
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
    <label className="flex cursor-pointer items-center justify-between gap-2 py-[3px] text-[12px] text-[#CBD5E1] transition hover:text-white">
      <span className="inline-flex min-w-0 items-center gap-2">
        <input type="radio" name={name} checked={checked} onChange={onSelect} className="sr-only" />
        <span
          className={[
            "flex h-3.5 w-3.5 shrink-0 items-center justify-center rounded-full border transition",
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
        <span className="shrink-0 rounded-md bg-white/[0.04] px-1.5 py-0.5 text-[10px] font-semibold text-[#64748B]">
          {count.toLocaleString("fa-IR")}
        </span>
      ) : null}
    </label>
  );
}

/** Dense filter sidebar — content-height only. */
export function ArticleFilter({ value, onChange, articles = [] }: Props) {
  const topicCounts = ARTICLE_SIDEBAR_TOPICS.map((item) => ({
    ...item,
    count: articles.filter((article) => article.category === item.id).length,
  }));

  return (
    <aside
      className="h-fit rounded-[14px] border border-white/[0.08] bg-[#0f172a]/80 p-2.5 shadow-[0_8px_28px_rgba(2,6,23,0.35)] backdrop-blur-xl"
      dir="rtl"
    >
      <div>
        <p className="mb-1 text-[11px] font-bold tracking-wide text-[#94A3B8]">جستجو در مقالات</p>
        <div className="relative">
          <span className="pointer-events-none absolute inset-y-0 left-2 flex items-center text-[#64748B]">
            <SearchIcon className="h-3.5 w-3.5" />
          </span>
          <input
            type="search"
            value={value.query}
            onChange={(event) => onChange({ ...value, query: event.target.value })}
            placeholder="جستجوی مقاله..."
            className="h-8 w-full rounded-[9px] border border-white/[0.1] bg-[#070b18]/80 px-2 pl-8 text-[12px] text-white outline-none transition placeholder:text-[#64748B] focus:border-[rgba(168,85,247,0.45)]"
          />
        </div>
      </div>

      <div className="mt-2 space-y-2">
        <FilterGroup title="موضوعات">
          {topicCounts.map((item) => (
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
            className="h-8 w-full rounded-[9px] border border-white/[0.1] bg-[#070b18]/80 px-2 text-[12px] text-white outline-none transition focus:border-[rgba(168,85,247,0.45)]"
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
