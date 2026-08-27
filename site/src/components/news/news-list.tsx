"use client";

import { useMemo, useState } from "react";
import { NewsArticleCard } from "@/components/news/news-article-card";
import { NEWS_TAGS } from "@/data/news-articles";
import type { NewsArticle, NewsTag } from "@/types";

type NewsListProps = {
  articles: NewsArticle[];
};

type FilterValue = "همه" | NewsTag;

const FILTERS: FilterValue[] = ["همه", ...NEWS_TAGS];

export function NewsList({ articles }: NewsListProps) {
  const [filter, setFilter] = useState<FilterValue>("همه");

  const visible = useMemo(() => {
    if (filter === "همه") return articles;
    return articles.filter((article) => article.tag === filter);
  }, [articles, filter]);

  return (
    <div className="space-y-5">
      <div
        className="flex flex-wrap gap-2"
        role="tablist"
        aria-label="Filter by topic"
      >
        {FILTERS.map((item) => {
          const isActive = filter === item;

          return (
            <button
              key={item}
              type="button"
              role="tab"
              aria-selected={isActive}
              onClick={() => setFilter(item)}
              className={[
                "ui-chip px-3.5 py-1.5",
                isActive ? "ui-chip-active" : "",
              ].join(" ")}
            >
              {item}
            </button>
          );
        })}
      </div>

      <p className="ui-meta">
        {visible.length} مطلب
        {filter !== "همه" ? ` در ${filter}` : ""}
      </p>

      {visible.length > 0 ? (
        <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
          {visible.map((article) => (
            <NewsArticleCard key={article.id} article={article} />
          ))}
        </div>
      ) : (
        <div className="ui-panel border-dashed px-4 py-12 text-center">
          <p className="ui-body">مطلبی برای این فیلتر پیدا نشد.</p>
        </div>
      )}
    </div>
  );
}
