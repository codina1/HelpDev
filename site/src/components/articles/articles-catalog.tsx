"use client";

import { useMemo, useState } from "react";
import { ArticleCard } from "@/components/articles/article-card";
import {
  ArticleFilter,
  DEFAULT_ARTICLES_FILTERS,
  type ArticlesFiltersState,
} from "@/components/articles/article-filter";
import { ArticlesContainer } from "@/components/articles/articles-container";
import { ArticleCategoryChipBar } from "@/components/articles/category-chip";
import { FeaturedArticle } from "@/components/articles/featured-article";
import { ArticlesPagination } from "@/components/articles/pagination";
import {
  ARTICLES_DISPLAY_TOTAL,
  ARTICLES_PAGE_SIZE,
  MARKETPLACE_ARTICLES,
  type ArticleCategoryId,
} from "@/data/articles";

function toFa(value: number): string {
  return value.toLocaleString("fa-IR");
}

/**
 * Articles marketplace catalog — chips · sidebar 280px · featured · 4-col grid · pagination.
 * Generous 48-64px section spacing.
 */
export function ArticlesCatalog() {
  const [quickCategory, setQuickCategory] = useState<ArticleCategoryId>("all");
  const [filters, setFilters] = useState<ArticlesFiltersState>(DEFAULT_ARTICLES_FILTERS);
  const [page, setPage] = useState(1);
  const [mobileFiltersOpen, setMobileFiltersOpen] = useState(false);

  const visible = useMemo(() => {
    const query = filters.query.trim().toLowerCase();
    const next = MARKETPLACE_ARTICLES.filter((article) => {
      if (quickCategory !== "all" && article.category !== quickCategory) return false;
      if (filters.topic !== "all" && article.category !== filters.topic) return false;
      if (filters.level !== "all" && article.level !== filters.level) return false;
      if (
        query &&
        !`${article.title} ${article.description} ${article.categoryLabel} ${article.author}`
          .toLowerCase()
          .includes(query)
      ) {
        return false;
      }
      return true;
    });

    switch (filters.sort) {
      case "popular":
        return next.slice().sort((a, b) => b.views - a.views);
      case "views":
        return next.slice().sort((a, b) => b.views - a.views || b.readingMinutes - a.readingMinutes);
      default:
        return next.slice().sort((a, b) => b.publishedAt.localeCompare(a.publishedAt));
    }
  }, [filters, quickCategory]);

  const featured = useMemo(() => {
    return visible.find((item) => item.featured) ?? visible[0] ?? null;
  }, [visible]);

  const gridSource = useMemo(() => {
    if (!featured) return visible;
    return visible.filter((item) => item.id !== featured.id);
  }, [featured, visible]);

  const isPristine =
    quickCategory === "all" &&
    !filters.query.trim() &&
    filters.topic === "all" &&
    filters.level === "all";

  const totalPages = isPristine
    ? Math.max(20, Math.ceil(ARTICLES_DISPLAY_TOTAL / ARTICLES_PAGE_SIZE))
    : Math.max(1, Math.ceil(gridSource.length / ARTICLES_PAGE_SIZE));
  const safePage = Math.min(page, totalPages);

  const pageItems = useMemo(() => {
    if (gridSource.length === 0) return [];
    if (!isPristine) {
      return gridSource.slice((safePage - 1) * ARTICLES_PAGE_SIZE, safePage * ARTICLES_PAGE_SIZE);
    }
    const start = (safePage - 1) * ARTICLES_PAGE_SIZE;
    return Array.from({ length: ARTICLES_PAGE_SIZE }, (_, index) => {
      return gridSource[(start + index) % gridSource.length];
    });
  }, [gridSource, isPristine, safePage]);

  const totalLabel = isPristine ? ARTICLES_DISPLAY_TOTAL : visible.length;

  return (
    <section id="articles-catalog" className="bg-[#070b18] pb-16 pt-0" dir="rtl">
      <ArticlesContainer>
        {/* Category chips — 48px gap below hero */}
        <div className="mb-12 min-w-0">
          <ArticleCategoryChipBar
            active={quickCategory}
            onSelect={(value) => {
              setQuickCategory(value);
              setPage(1);
            }}
          />
        </div>

        {/* Mobile filter toggle */}
        <div className="mb-5 flex items-center justify-between gap-3 lg:hidden">
          <button
            type="button"
            onClick={() => setMobileFiltersOpen((open) => !open)}
            className="inline-flex h-11 items-center gap-2 rounded-xl border border-white/[0.1] bg-[#0F1626] px-5 text-[13px] font-bold text-[#E5E7EB]"
          >
            فیلترها
          </button>
          <p className="text-[13px] font-semibold text-[#64748B]">{toFa(totalLabel)} مقاله</p>
        </div>

        {/* 280px sidebar left · articles right */}
        <div dir="ltr" className="grid grid-cols-1 gap-10 lg:grid-cols-[280px_minmax(0,1fr)]">
          <div className={mobileFiltersOpen ? "block" : "hidden lg:block"} dir="rtl">
            <div className="sticky top-6">
              <ArticleFilter
                value={filters}
                onChange={(next) => {
                  setFilters(next);
                  setPage(1);
                }}
              />
            </div>
          </div>

          <div className="min-w-0" dir="rtl">
            {/* Section header */}
            <div className="mb-6 hidden flex-wrap items-end justify-between gap-2 lg:flex">
              <div>
                <h2 className="text-[20px] font-extrabold text-white sm:text-[22px]">همه مقالات</h2>
                <p className="mt-1.5 text-[13px] font-semibold text-[#64748B]">
                  {toFa(totalLabel)} مقاله
                </p>
              </div>
            </div>

            {visible.length === 0 ? (
              <p
                className="rounded-[18px] border border-dashed border-white/[0.12] px-4 py-16 text-center text-[14px] text-[#94A3B8]"
                role="status"
              >
                مقاله‌ای با این فیلتر پیدا نشد.
              </p>
            ) : (
              <>
                {/* Featured card */}
                {featured && safePage === 1 ? (
                  <div className="mb-10">
                    <FeaturedArticle article={featured} />
                  </div>
                ) : null}

                {/* 4 column grid */}
                <ul className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                  {pageItems.map((article, index) => (
                    <li key={`${article.id}-${safePage}-${index}`}>
                      <ArticleCard article={article} />
                    </li>
                  ))}
                </ul>

                {/* Pagination — 48px top margin */}
                <div className="mt-12">
                  <ArticlesPagination page={safePage} totalPages={totalPages} onPageChange={setPage} />
                </div>
              </>
            )}
          </div>
        </div>
      </ArticlesContainer>
    </section>
  );
}
