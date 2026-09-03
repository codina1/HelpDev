"use client";

import { useMemo, useState } from "react";
import { PromptLabCard } from "@/components/prompt-lab/prompt-lab-card";
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
  PROMPT_LAB_PAGE_SIZE,
  PROMPT_LAB_QUICK_FILTERS,
  PROMPT_LAB_SAMPLE_PROMPTS,
  type PromptLabQuickFilterId,
} from "@/data/prompt-lab";

function toFa(value: number): string {
  return value.toLocaleString("fa-IR");
}

/**
 * Prompt Lab catalog — pills · left sidebar (260px) · 4×12 card grid · pagination.
 */
export function PromptLabCatalog() {
  const [quickFilter, setQuickFilter] = useState<PromptLabQuickFilterId>("all");
  const [filters, setFilters] = useState<PromptLabFiltersState>(DEFAULT_PROMPT_LAB_FILTERS);
  const [page, setPage] = useState(1);
  const [mobileFiltersOpen, setMobileFiltersOpen] = useState(false);

  const activeQuick = useMemo(
    () => PROMPT_LAB_QUICK_FILTERS.find((item) => item.id === quickFilter) ?? PROMPT_LAB_QUICK_FILTERS[0],
    [quickFilter],
  );

  const visible = useMemo(() => {
    const query = filters.query.trim().toLowerCase();
    const next = PROMPT_LAB_SAMPLE_PROMPTS.filter((item) => {
      if (activeQuick.category && item.category !== activeQuick.category) return false;
      if (activeQuick.aiModel && item.aiModel !== activeQuick.aiModel) return false;
      if (filters.categories.length > 0 && !filters.categories.includes(item.category)) return false;
      if (filters.models.length > 0 && !filters.models.includes(item.aiModel)) return false;
      if (
        query &&
        !`${item.title} ${item.description} ${item.category} ${item.aiModel}`.toLowerCase().includes(query)
      ) {
        return false;
      }
      return true;
    });

    switch (filters.sort) {
      case "popular":
        return next.slice().sort((a, b) => b.copyCount - a.copyCount);
      case "views":
        return next.slice().sort((a, b) => b.viewCount - a.viewCount);
      default:
        return next.slice().sort((a, b) => b.publishedAt.localeCompare(a.publishedAt));
    }
  }, [activeQuick, filters]);

  const totalPages = Math.max(1, Math.ceil(visible.length / PROMPT_LAB_PAGE_SIZE));
  const safePage = Math.min(page, totalPages);
  const pageItems = visible.slice((safePage - 1) * PROMPT_LAB_PAGE_SIZE, safePage * PROMPT_LAB_PAGE_SIZE);

  const isPristine =
    quickFilter === "all" &&
    !filters.query.trim() &&
    filters.categories.length === 0 &&
    filters.models.length === 0;
  const totalLabel = isPristine ? PROMPT_LAB_DISPLAY_TOTAL : visible.length;

  return (
    <section id="prompt-lab-catalog" className="bg-[#070b18] pb-2 pt-0" dir="rtl">
      <PromptLabContainer>
        <div className="mb-6">
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

        {/* LTR row so sidebar sits on the visual LEFT, grid on the RIGHT */}
        <div
          className="grid grid-cols-1 gap-8 lg:grid-cols-[260px_minmax(0,1fr)]"
          dir="ltr"
        >
          <div className={mobileFiltersOpen ? "block" : "hidden lg:block"} dir="rtl">
            <PromptLabFiltersSidebar
              value={filters}
              onChange={(next) => {
                setFilters(next);
                setPage(1);
              }}
            />
          </div>

          <div className="min-w-0" dir="rtl">
            <div className="mb-4 flex flex-wrap items-end justify-between gap-2">
              <div>
                <h2 className="text-[18px] font-extrabold text-white sm:text-[20px]">همه پرامپت‌ها</h2>
                <p className="mt-1 text-[12.5px] font-semibold text-[#64748B]">
                  {toFa(totalLabel)} پرامپت
                </p>
              </div>
            </div>

            {pageItems.length === 0 ? (
              <p
                className="rounded-[16px] border border-dashed border-white/[0.12] px-4 py-12 text-center text-[13px] text-[#94A3B8]"
                role="status"
              >
                پرامپتی با این فیلتر پیدا نشد.
              </p>
            ) : (
              <>
                <ul className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
                  {pageItems.map((item) => (
                    <li key={item.id}>
                      <PromptLabCard item={item} />
                    </li>
                  ))}
                </ul>
                <PromptLabPagination
                  page={safePage}
                  totalPages={totalPages}
                  onPageChange={setPage}
                />
              </>
            )}
          </div>
        </div>
      </PromptLabContainer>
    </section>
  );
}
