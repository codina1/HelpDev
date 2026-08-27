"use client";

import { useMemo, useState } from "react";
import { FeaturedNews } from "@/components/news/featured-news";
import { NewsArticleCard } from "@/components/news/news-article-card";
import { NewsCategoryFilter } from "@/components/news/news-category-filter";
import { NewsContainer } from "@/components/news/news-container";
import { PopularNewsSidebar } from "@/components/news/popular-news-sidebar";
import { TagsSidebar } from "@/components/news/tags-sidebar";
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

  return (
    <section className="bg-[#050816] pb-12 pt-2" dir="rtl">
      <NewsContainer>
        <div className="mb-7">
          <NewsCategoryFilter
            active={category}
            onSelect={(value) => {
              setCategory(value);
              setPage(1);
            }}
          />
        </div>

        {/* Right content ~900px · Left sidebar 260px · gap ~40px */}
        <div className="grid items-start gap-6 lg:grid-cols-[minmax(0,1fr)_260px] lg:gap-10">
          <div className="order-1 min-w-0">
            {pageItems.length > 0 ? (
              <>
                <div className="space-y-5">
                  <FeaturedNews article={pageItems[0]} />
                  {pageItems.length > 1 ? (
                    <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
                      {pageItems.slice(1).map((article) => (
                        <NewsArticleCard key={article.id} article={article} />
                      ))}
                    </div>
                  ) : null}
                </div>
                <NewsPagination
                  currentPage={currentPage}
                  totalPages={Math.max(totalPages, 5)}
                  onPageChange={setPage}
                />
              </>
            ) : (
              <div className="rounded-[16px] border border-dashed border-white/[0.12] px-4 py-12 text-center text-[14px] text-[#94A3B8]">
                مطلبی برای این فیلتر پیدا نشد.
              </div>
            )}
          </div>

          <aside className="order-2 grid grid-cols-1 gap-5 sm:grid-cols-2 lg:sticky lg:top-24 lg:grid-cols-1">
            <PopularNewsSidebar />
            <TagsSidebar
              activeTag={cloudTag}
              onTagSelect={(value) => {
                setCloudTag(value);
                setPage(1);
              }}
            />
          </aside>
        </div>
      </NewsContainer>
    </section>
  );
}

function NewsPagination({
  currentPage,
  totalPages,
  onPageChange,
}: {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}) {
  const pages = [1, 2, 3, 4, 5];

  return (
    <nav className="flex flex-wrap items-center justify-center gap-1.5 pt-8" aria-label="صفحه‌بندی اخبار">
      <button
        type="button"
        className="inline-flex h-9 w-9 items-center justify-center rounded-lg border border-white/[0.08] text-[#94A3B8] transition hover:text-white disabled:opacity-40"
        onClick={() => onPageChange(Math.max(1, currentPage - 1))}
        disabled={currentPage === 1}
        aria-label="صفحه قبلی"
      >
        ‹
      </button>
      {pages.map((page) => (
        <button
          key={page}
          type="button"
          aria-current={page === currentPage ? "page" : undefined}
          onClick={() => onPageChange(page)}
          className={[
            "inline-flex h-9 min-w-9 items-center justify-center rounded-lg px-2 text-[13px] font-semibold transition",
            page === currentPage
              ? "bg-[#7C3AED] text-white shadow-[0_0_16px_rgba(124,58,237,0.35)]"
              : "text-[#94A3B8] hover:text-white",
          ].join(" ")}
        >
          {page}
        </button>
      ))}
      <span className="px-1 text-[13px] text-[#64748B]">…</span>
      <button
        type="button"
        className="inline-flex h-9 min-w-9 items-center justify-center rounded-lg px-2 text-[13px] font-semibold text-[#94A3B8] hover:text-white"
        onClick={() => onPageChange(20)}
      >
        ۲۰
      </button>
      <button
        type="button"
        className="inline-flex h-9 w-9 items-center justify-center rounded-lg border border-white/[0.08] text-[#94A3B8] transition hover:text-white disabled:opacity-40"
        onClick={() => onPageChange(Math.min(totalPages, currentPage + 1))}
        disabled={currentPage >= totalPages}
        aria-label="صفحه بعدی"
      >
        ›
      </button>
    </nav>
  );
}
