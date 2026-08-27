"use client";

import { useMemo, useState } from "react";
import { FeaturedNews } from "@/components/news/featured-news";
import { NewsArticleCard } from "@/components/news/news-article-card";
import { NewsCategoryFilter } from "@/components/news/news-category-filter";
import { PopularNewsSidebar } from "@/components/news/popular-news-sidebar";
import { TagsSidebar } from "@/components/news/tags-sidebar";
import { PublicContainer } from "@/components/ui/public/v2/public-container";
import {
  filterNewsArticles,
  type NewsCategoryId,
  type NewsCloudTag,
} from "@/data/news-articles";
import type { NewsArticle } from "@/types";

type NewsListProps = {
  articles: NewsArticle[];
};

const PAGE_SIZE = 7;

export function NewsList({ articles }: NewsListProps) {
  const [category, setCategory] = useState<NewsCategoryId>("همه");
  const [cloudTag, setCloudTag] = useState<NewsCloudTag>("همه");
  const [page, setPage] = useState(1);

  const visible = useMemo(
    () => filterNewsArticles(articles, category, cloudTag),
    [articles, category, cloudTag],
  );
  const totalPages = Math.max(1, Math.ceil(visible.length / PAGE_SIZE));
  const currentPage = Math.min(page, totalPages);
  const pageItems = useMemo(
    () => visible.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE),
    [currentPage, visible],
  );

  function selectCategory(value: NewsCategoryId) {
    setCategory(value);
    setPage(1);
  }

  function selectCloudTag(value: NewsCloudTag) {
    setCloudTag(value);
    setPage(1);
  }

  return (
    <section className="bg-[#050816] pb-12 pt-1 sm:pb-14 sm:pt-2" dir="rtl">
      <PublicContainer size="wide">
        {/* Category nav — directly under hero */}
        <div className="mb-7 sm:mb-8">
          <NewsCategoryFilter active={category} onSelect={selectCategory} />
        </div>

        {/* Main content (right in RTL) + fixed left sidebar column */}
        <div className="grid items-start gap-6 lg:grid-cols-[minmax(0,1fr)_300px] lg:gap-8">
          <div className="order-1 min-w-0">
            <div className="mb-5 flex flex-wrap items-end justify-between gap-3">
              <div>
                <p className="text-[12px] font-bold tracking-wide text-[#A78BFA]">تازه‌ترین مطالب</p>
                <p className="mt-1 text-[12px] font-medium text-[#64748B] sm:text-[13px]">
                  {visible.length} مطلب
                  {category !== "همه" ? ` در ${category}` : ""}
                  {cloudTag !== "همه" ? ` · #${cloudTag}` : ""}
                </p>
              </div>
            </div>

            {pageItems.length > 0 ? (
              <>
                <div className="space-y-5 sm:space-y-6">
                  <FeaturedNews article={pageItems[0]} />
                  {pageItems.length > 1 ? (
                    <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3 lg:gap-6">
                      {pageItems.slice(1).map((article) => (
                        <NewsArticleCard key={article.id} article={article} />
                      ))}
                    </div>
                  ) : null}
                </div>
                <NewsPagination
                  currentPage={currentPage}
                  totalPages={totalPages}
                  onPageChange={setPage}
                />
              </>
            ) : (
              <div className="rounded-[20px] border border-dashed border-white/[0.12] bg-[#111827]/60 px-4 py-12 text-center">
                <p className="text-[14px] text-[#94A3B8]">مطلبی برای این فیلتر پیدا نشد.</p>
              </div>
            )}
          </div>

          <aside className="order-2 grid grid-cols-1 gap-5 sm:grid-cols-2 lg:sticky lg:top-24 lg:grid-cols-1 lg:gap-5">
            <PopularNewsSidebar articles={articles} />
            <TagsSidebar activeTag={cloudTag} onTagSelect={selectCloudTag} />
          </aside>
        </div>
      </PublicContainer>
    </section>
  );
}

type NewsPaginationProps = {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
};

function NewsPagination({ currentPage, totalPages, onPageChange }: NewsPaginationProps) {
  if (totalPages <= 1) return null;

  return (
    <nav
      className="flex flex-wrap items-center justify-center gap-1.5 pt-5 sm:gap-2 sm:pt-6"
      aria-label="صفحه‌بندی اخبار"
    >
      <button
        type="button"
        className="rounded-lg border border-white/[0.08] bg-white/[0.03] px-2.5 py-1.5 text-[12px] font-semibold text-[#94A3B8] transition hover:border-[rgba(168,85,247,0.35)] hover:text-white disabled:cursor-not-allowed disabled:opacity-40 sm:px-3"
        onClick={() => onPageChange(currentPage - 1)}
        disabled={currentPage === 1}
        aria-label="صفحه قبلی"
      >
        قبلی
      </button>
      <div className="flex max-w-full flex-wrap items-center justify-center gap-1 sm:gap-1.5" role="list">
        {Array.from({ length: totalPages }, (_, index) => index + 1).map((page) => (
          <button
            key={page}
            type="button"
            role="listitem"
            aria-current={page === currentPage ? "page" : undefined}
            onClick={() => onPageChange(page)}
            className={[
              "min-w-8 rounded-lg border px-2 py-1.5 text-[12px] font-semibold transition sm:min-w-9 sm:px-2.5",
              page === currentPage
                ? "border-[rgba(168,85,247,0.5)] bg-[rgba(124,58,237,0.28)] text-white shadow-[0_0_16px_rgba(124,58,237,0.2)]"
                : "border-white/[0.08] bg-white/[0.03] text-[#94A3B8] hover:border-[rgba(168,85,247,0.35)] hover:text-white",
            ].join(" ")}
          >
            {page}
          </button>
        ))}
      </div>
      <button
        type="button"
        className="rounded-lg border border-white/[0.08] bg-white/[0.03] px-2.5 py-1.5 text-[12px] font-semibold text-[#94A3B8] transition hover:border-[rgba(168,85,247,0.35)] hover:text-white disabled:cursor-not-allowed disabled:opacity-40 sm:px-3"
        onClick={() => onPageChange(currentPage + 1)}
        disabled={currentPage === totalPages}
        aria-label="صفحه بعدی"
      >
        بعدی
      </button>
    </nav>
  );
}
