"use client";

import { useMemo, useState } from "react";
import { FeaturedNews } from "@/components/news/featured-news";
import { NewsArticleCard } from "@/components/news/news-article-card";
import { PopularNewsSidebar } from "@/components/news/popular-news-sidebar";
import { TagsSidebar } from "@/components/news/tags-sidebar";
import { NEWS_TAGS } from "@/data/news-articles";
import type { NewsArticle, NewsTag } from "@/types";

type NewsListProps = {
  articles: NewsArticle[];
};

type FilterValue = "همه" | NewsTag;

const FILTERS: FilterValue[] = ["همه", ...NEWS_TAGS];
const PAGE_SIZE = 7;

export function NewsList({ articles }: NewsListProps) {
  const [filter, setFilter] = useState<FilterValue>("همه");
  const [page, setPage] = useState(1);

  const visible = useMemo(() => {
    if (filter === "همه") return articles;
    return articles.filter((article) => article.tag === filter);
  }, [articles, filter]);
  const totalPages = Math.max(1, Math.ceil(visible.length / PAGE_SIZE));
  const currentPage = Math.min(page, totalPages);
  const pageItems = useMemo(
    () => visible.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE),
    [currentPage, visible],
  );

  function selectFilter(value: FilterValue) {
    setFilter(value);
    setPage(1);
  }

  return (
    <div className="space-y-6" dir="rtl">
      <div className="grid items-start gap-6 lg:grid-cols-[minmax(0,1fr)_280px] lg:gap-7">
        <main className="min-w-0">
          <div className="mb-5 flex flex-wrap items-end justify-between gap-3">
            <div>
              <p className="text-[12px] font-bold tracking-wide text-[#A78BFA]">تازه‌ترین مطالب</p>
              <p className="ui-meta mt-1">
                {visible.length} مطلب
                {filter !== "همه" ? ` در ${filter}` : ""}
              </p>
            </div>
          </div>

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
                totalPages={totalPages}
                onPageChange={setPage}
              />
            </>
          ) : (
            <div className="ui-panel border-dashed px-4 py-12 text-center">
              <p className="ui-body">مطلبی برای این فیلتر پیدا نشد.</p>
            </div>
          )}
        </main>

        <aside className="order-2 space-y-5 lg:sticky lg:top-24">
          <PopularNewsSidebar articles={articles} />
          <TagsSidebar tags={FILTERS} activeTag={filter} onTagSelect={selectFilter} />
        </aside>
      </div>
    </div>
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
    <nav className="flex items-center justify-center gap-2 pt-5" aria-label="صفحه‌بندی اخبار">
      <button
        type="button"
        className="ui-chip px-3 py-1.5 disabled:cursor-not-allowed disabled:opacity-40"
        onClick={() => onPageChange(currentPage - 1)}
        disabled={currentPage === 1}
        aria-label="صفحه قبلی"
      >
        قبلی
      </button>
      <div className="flex items-center gap-1.5" role="list">
        {Array.from({ length: totalPages }, (_, index) => index + 1).map((page) => (
          <button
            key={page}
            type="button"
            role="listitem"
            aria-current={page === currentPage ? "page" : undefined}
            onClick={() => onPageChange(page)}
            className={[
              "ui-chip min-w-9 px-2.5 py-1.5",
              page === currentPage ? "ui-chip-active" : "",
            ].join(" ")}
          >
            {page}
          </button>
        ))}
      </div>
      <button
        type="button"
        className="ui-chip px-3 py-1.5 disabled:cursor-not-allowed disabled:opacity-40"
        onClick={() => onPageChange(currentPage + 1)}
        disabled={currentPage === totalPages}
        aria-label="صفحه بعدی"
      >
        بعدی
      </button>
    </nav>
  );
}
