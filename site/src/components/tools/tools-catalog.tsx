"use client";

import { useMemo, useState } from "react";
import { CategoryChipBar } from "@/components/tools/category-chip";
import { ToolCard } from "@/components/tools/tool-card";
import {
  DEFAULT_TOOLS_FILTERS,
  ToolFilter,
  type ToolsFiltersState,
} from "@/components/tools/tool-filter";
import { ToolsPagination } from "@/components/tools/pagination";
import { ToolsContainer } from "@/components/tools/tools-container";
import {
  MARKETPLACE_TOOLS,
  TOOLS_DISPLAY_TOTAL,
  TOOLS_PAGE_SIZE,
  type ToolCategoryId,
} from "@/data/tools";

function toFa(value: number): string {
  return value.toLocaleString("fa-IR");
}

/**
 * Tools marketplace catalog — chips · sidebar · 4-col grid · pagination.
 */
export function ToolsCatalog() {
  const [quickCategory, setQuickCategory] = useState<ToolCategoryId>("all");
  const [filters, setFilters] = useState<ToolsFiltersState>(DEFAULT_TOOLS_FILTERS);
  const [page, setPage] = useState(1);
  const [mobileFiltersOpen, setMobileFiltersOpen] = useState(false);

  const visible = useMemo(() => {
    const query = filters.query.trim().toLowerCase();
    const next = MARKETPLACE_TOOLS.filter((tool) => {
      if (quickCategory !== "all" && tool.category !== quickCategory) return false;
      if (filters.categories.length > 0 && !filters.categories.includes(tool.category)) return false;
      if (filters.price !== "all" && tool.price !== filters.price) return false;
      if (query && !`${tool.name} ${tool.description} ${tool.categoryLabel}`.toLowerCase().includes(query)) {
        return false;
      }
      return true;
    });

    switch (filters.sort) {
      case "popular":
        return next.slice().sort((a, b) => b.reviewCount - a.reviewCount);
      case "rating":
        return next.slice().sort((a, b) => b.rating - a.rating || b.reviewCount - a.reviewCount);
      default:
        return next.slice().sort((a, b) => b.publishedAt.localeCompare(a.publishedAt));
    }
  }, [filters, quickCategory]);

  const totalPages = Math.max(1, Math.ceil(visible.length / TOOLS_PAGE_SIZE));
  const safePage = Math.min(page, totalPages);
  const pageItems = visible.slice((safePage - 1) * TOOLS_PAGE_SIZE, safePage * TOOLS_PAGE_SIZE);

  const isPristine =
    quickCategory === "all" &&
    !filters.query.trim() &&
    filters.categories.length === 0 &&
    filters.price === "all";
  const totalLabel = isPristine ? TOOLS_DISPLAY_TOTAL : visible.length;

  return (
    <section id="tools-catalog" className="bg-[#070b18] pb-2 pt-0" dir="rtl">
      <ToolsContainer>
        <div className="mb-6 min-w-0">
          <CategoryChipBar
            active={quickCategory}
            onSelect={(value) => {
              setQuickCategory(value);
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

        {/* RTL: sidebar first → right side; tools grid → left side */}
        <div className="grid grid-cols-1 gap-8 lg:grid-cols-[260px_minmax(0,1fr)]">
          <div className={mobileFiltersOpen ? "block" : "hidden lg:block"}>
            <ToolFilter
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
                <h2 className="text-[18px] font-extrabold text-white sm:text-[20px]">همه ابزارها</h2>
                <p className="mt-1 text-[12.5px] font-semibold text-[#64748B]">
                  {toFa(totalLabel)} ابزار
                </p>
              </div>
            </div>

            {pageItems.length === 0 ? (
              <p
                className="rounded-[16px] border border-dashed border-white/[0.12] px-4 py-12 text-center text-[13px] text-[#94A3B8]"
                role="status"
              >
                ابزاری با این فیلتر پیدا نشد.
              </p>
            ) : (
              <>
                <ul className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
                  {pageItems.map((tool) => (
                    <li key={tool.id}>
                      <ToolCard tool={tool} />
                    </li>
                  ))}
                </ul>
                <ToolsPagination page={safePage} totalPages={totalPages} onPageChange={setPage} />
              </>
            )}
          </div>
        </div>
      </ToolsContainer>
    </section>
  );
}
