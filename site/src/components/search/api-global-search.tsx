"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { useSearchParams } from "next/navigation";
import { PageEmptyState } from "@/components/ui/page-empty-state";
import { PageErrorState } from "@/components/ui/page-error-state";
import { PageLoadingState } from "@/components/ui/page-loading-state";
import { search, type SearchResultItemDto } from "@/lib/api/search";
import { hrefForSearchResult, labelForSearchSource } from "@/lib/public/search-navigation";

const TABS = [
  { value: "", label: "همه" },
  { value: "content", label: "مقالات" },
  { value: "tool", label: "ابزارها" },
  { value: "course", label: "دوره‌ها" },
  { value: "lesson", label: "درس" },
  { value: "prompt", label: "پرامپت" },
] as const;

/** API-backed global search page (replaces static local index usage). */
export function ApiGlobalSearch() {
  const searchParams = useSearchParams();
  const initialQ = searchParams.get("q") ?? "";
  const [query, setQuery] = useState(initialQ);
  const [type, setType] = useState(searchParams.get("type") ?? "");
  const [items, setItems] = useState<SearchResultItemDto[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<unknown>(null);

  const run = useCallback(async (q: string, sourceType: string) => {
    const trimmed = q.trim();
    if (!trimmed) {
      setItems([]);
      setTotal(0);
      setError(null);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const result = await search({
        q: trimmed,
        type: sourceType || undefined,
        page: 1,
        pageSize: 20,
      });
      setItems(result.items ?? []);
      setTotal(result.total ?? result.items?.length ?? 0);
    } catch (err) {
      setError(err);
      setItems([]);
      setTotal(0);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    setQuery(initialQ);
  }, [initialQ]);

  useEffect(() => {
    const handle = window.setTimeout(() => {
      void run(query, type);
    }, 250);
    return () => window.clearTimeout(handle);
  }, [query, type, run]);

  return (
    <div className="mx-auto max-w-3xl space-y-5" dir="rtl">
      <header>
        <h1 className="text-2xl font-extrabold text-[color:var(--foreground)]">جستجو</h1>
        <p className="mt-1 text-sm text-[color:var(--muted)]">
          مقالات، ابزارها، نقشه راه و دوره‌ها — از Search API · Ctrl+K برای پالت سریع
        </p>
      </header>

      <label className="block space-y-1">
        <span className="sr-only">عبارت جستجو</span>
        <input
          type="search"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="جستجو..."
          className="focus-ring w-full rounded-xl border border-[color:var(--border-strong)] bg-[color:var(--surface-elevated)] px-3 py-2.5 text-sm text-[color:var(--foreground)]"
        />
      </label>

      <div className="flex flex-wrap gap-1.5" role="tablist" aria-label="نوع منبع">
        {TABS.map((tab) => (
          <button
            key={tab.value || "all"}
            type="button"
            role="tab"
            aria-selected={type === tab.value}
            onClick={() => setType(tab.value)}
            className={[
              "focus-ring rounded-lg px-2.5 py-1 text-[11px] font-semibold",
              type === tab.value
                ? "bg-[color:var(--accent-soft)] text-violet-100"
                : "bg-[color:var(--surface)] text-[color:var(--muted)]",
            ].join(" ")}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {loading ? (
        <PageLoadingState rows={5} />
      ) : error ? (
        <PageErrorState error={error} onRetry={() => void run(query, type)} />
      ) : !query.trim() ? (
        <PageEmptyState title="عبارت جستجو را وارد کنید" description="Ctrl+K نیز جستجوی سریع را باز می‌کند." />
      ) : items.length === 0 ? (
        <PageEmptyState title="نتیجه‌ای نیست" description={`${total} نتیجه برای این عبارت.`} />
      ) : (
        <ul className="space-y-2">
          <li className="text-[12px] text-[color:var(--muted)]">{total} نتیجه</li>
          {items.map((item) => (
            <li key={`${item.sourceType}-${item.sourceId}`}>
              <Link
                href={hrefForSearchResult(item)}
                className="focus-ring block rounded-2xl border border-[color:var(--border)] bg-[color:var(--surface)] p-4 transition hover:border-[color:color-mix(in_srgb,var(--accent)_35%,transparent)]"
              >
                <div className="flex items-center justify-between gap-2">
                  <h2 className="text-[15px] font-bold text-[color:var(--foreground)]">{item.title}</h2>
                  <span className="rounded bg-[color:var(--surface-elevated)] px-2 py-0.5 text-[10px] font-bold text-[color:var(--muted)]">
                    {labelForSearchSource(item)}
                  </span>
                </div>
                {item.summary ? (
                  <p className="mt-2 text-[13px] leading-6 text-[color:var(--muted)]">{item.summary}</p>
                ) : null}
              </Link>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
