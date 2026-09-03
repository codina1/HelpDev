"use client";

import { useEffect, useMemo, useState } from "react";
import { PromptLabCard, PromptLabCardSkeleton } from "@/components/prompt-lab/prompt-lab-card";
import { PromptLabCategoryBar } from "@/components/prompt-lab/prompt-lab-category-bar";
import { PromptLabContainer } from "@/components/prompt-lab/prompt-lab-container";
import {
  DEFAULT_PROMPT_LAB_FILTERS,
  PromptLabFiltersSidebar,
  type PromptLabFiltersState,
} from "@/components/prompt-lab/prompt-lab-filters-sidebar";
import { PromptLabPagination } from "@/components/prompt-lab/prompt-lab-pagination";
import {
  PROMPT_LAB_DISPLAY_TOTAL,
  PROMPT_LAB_QUICK_FILTERS,
  type PromptLabQuickFilterId,
} from "@/data/prompt-lab";
import { ApiClientError } from "@/lib/api/errors";
import {
  EMPTY_PROMPT_LAB_CATALOG_PAGE,
  fetchPromptLabCatalog,
  PROMPT_LAB_PAGE_SIZE,
  type PromptLabCatalogPage,
} from "@/lib/public/prompt-lab-catalog";

function isCatalogAbort(error: unknown): boolean {
  if (error instanceof ApiClientError && error.code === "request_aborted") return true;
  if (error instanceof DOMException) return error.name === "AbortError";
  return error instanceof Error && error.name === "AbortError";
}

function toFa(value: number): string {
  return value.toLocaleString("fa-IR");
}

/**
 * Prompt Lab catalog — category pills · sidebar filters · 4-col grid · pagination.
 */
export function PromptLabCatalog() {
  const [quickFilter, setQuickFilter] = useState<PromptLabQuickFilterId>("all");
  const [filters, setFilters] = useState<PromptLabFiltersState>(DEFAULT_PROMPT_LAB_FILTERS);
  const [debouncedQuery, setDebouncedQuery] = useState("");
  const [page, setPage] = useState(1);
  const [catalog, setCatalog] = useState<PromptLabCatalogPage>(EMPTY_PROMPT_LAB_CATALOG_PAGE);
  const [status, setStatus] = useState<"loading" | "error" | "ready">("loading");
  const [reloadKey, setReloadKey] = useState(0);
  const [mobileFiltersOpen, setMobileFiltersOpen] = useState(false);

  const activeQuick = useMemo(
    () => PROMPT_LAB_QUICK_FILTERS.find((item) => item.id === quickFilter) ?? PROMPT_LAB_QUICK_FILTERS[0],
    [quickFilter],
  );

  const categorySlug =
    filters.categories[0] ?? activeQuick.category ?? null;
  const aiModelSlug = filters.models[0] ?? activeQuick.aiModel ?? null;
  const popular = filters.sort === "popular" || filters.sort === "views";

  useEffect(() => {
    const next = filters.query.trim();
    const timer = window.setTimeout(() => {
      setDebouncedQuery((current) => {
        if (current !== next) setPage(1);
        return next;
      });
    }, 300);
    return () => window.clearTimeout(timer);
  }, [filters.query]);

  useEffect(() => {
    const controller = new AbortController();
    setStatus("loading");

    fetchPromptLabCatalog({
      search: debouncedQuery,
      category: categorySlug,
      aiModel: aiModelSlug,
      page,
      pageSize: PROMPT_LAB_PAGE_SIZE,
      popular,
      signal: controller.signal,
    })
      .then((result) => {
        setCatalog(result);
        setStatus("ready");
      })
      .catch((error: unknown) => {
        if (isCatalogAbort(error)) return;
        setStatus("error");
      });

    return () => controller.abort();
  }, [debouncedQuery, categorySlug, aiModelSlug, page, popular, reloadKey]);

  const totalPages = Math.max(1, Math.ceil(catalog.total / Math.max(1, catalog.pageSize)));
  const isPristine =
    quickFilter === "all" &&
    !debouncedQuery &&
    filters.categories.length === 0 &&
    filters.models.length === 0;
  const totalLabel = isPristine ? PROMPT_LAB_DISPLAY_TOTAL : catalog.total;

  return (
    <section id="prompt-lab-catalog" className="bg-[#070b18] pb-8 pt-2" dir="rtl">
      <PromptLabContainer>
        <div className="mb-6 mt-0 md:mb-6">
          <PromptLabCategoryBar
            active={quickFilter}
            onSelect={(value) => {
              setQuickFilter(value);
              setPage(1);
            }}
          />
        </div>

        <div className="mb-4 flex items-center justify-between gap-3 lg:hidden">
          <button
            type="button"
            onClick={() => setMobileFiltersOpen((open) => !open)}
            className="inline-flex h-10 items-center gap-2 rounded-xl border border-white/[0.1] bg-[#0F1626] px-4 text-[13px] font-bold text-[#E5E7EB]"
          >
            فیلترها
          </button>
        </div>

        <div className="grid grid-cols-1 gap-5 lg:grid-cols-[240px_minmax(0,1fr)] lg:gap-5">
          <div className={mobileFiltersOpen ? "block" : "hidden lg:block"}>
            <PromptLabFiltersSidebar
              value={filters}
              onChange={(next) => {
                setFilters(next);
                setPage(1);
              }}
            />
          </div>

          <div className="min-w-0">
            <div className="mb-4 flex flex-wrap items-end justify-between gap-2">
              <div>
                <h2 className="text-[18px] font-extrabold text-white sm:text-[20px]">همه پرامپت‌ها</h2>
                <p className="mt-1 text-[12.5px] font-semibold text-[#64748B]">
                  {toFa(totalLabel)} پرامپت
                </p>
              </div>
            </div>

            {status === "loading" ? (
              <ul className="grid grid-cols-1 gap-3.5 sm:grid-cols-2 lg:grid-cols-4" aria-busy="true">
                {Array.from({ length: PROMPT_LAB_PAGE_SIZE }, (_, index) => (
                  <li key={`skeleton-${index}`}>
                    <PromptLabCardSkeleton />
                  </li>
                ))}
              </ul>
            ) : null}

            {status === "error" ? (
              <div className="rounded-[16px] border border-dashed border-white/[0.12] px-4 py-12 text-center" role="alert">
                <p className="text-[13px] text-[#94A3B8]">بارگذاری پرامپت‌ها ناموفق بود.</p>
                <button
                  type="button"
                  onClick={() => setReloadKey((value) => value + 1)}
                  className="mt-4 inline-flex h-10 items-center rounded-xl border border-white/[0.1] bg-[#0F1626] px-4 text-[13px] font-bold text-white"
                >
                  تلاش مجدد
                </button>
              </div>
            ) : null}

            {status === "ready" && catalog.items.length === 0 ? (
              <p className="rounded-[16px] border border-dashed border-white/[0.12] px-4 py-12 text-center text-[13px] text-[#94A3B8]" role="status">
                پرامپتی با این فیلتر پیدا نشد.
              </p>
            ) : null}

            {status === "ready" && catalog.items.length > 0 ? (
              <>
                <ul className="grid grid-cols-1 gap-3.5 sm:grid-cols-2 lg:grid-cols-4">
                  {catalog.items.map((item) => (
                    <li key={item.id}>
                      <PromptLabCard item={item} />
                    </li>
                  ))}
                </ul>
                <PromptLabPagination page={page} totalPages={totalPages} onPageChange={setPage} />
              </>
            ) : null}
          </div>
        </div>
      </PromptLabContainer>
    </section>
  );
}
