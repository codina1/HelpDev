"use client";

import { useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { PageEmptyState } from "@/components/ui/page-empty-state";
import {
  GradientText,
  KnowledgeCard,
  PremiumBadge,
  PublicContainer,
  PublicSection,
  GlowButton,
} from "@/components/ui/public/v2";
import type { ContentSummaryDto } from "@/lib/api/content";
import { formatDateFa, labelForContentType } from "@/lib/admin/content/content-mappers";
import { estimateReadingLabel, softDifficulty } from "@/lib/public/display-meta";
import { publicHrefForContent } from "@/lib/public/content-helpers";

const PAGE_SIZE = 9;

const TYPE_FILTERS = [
  { value: "all", label: "همه" },
  { value: "Article", label: "مقاله" },
  { value: "News", label: "خبر" },
] as const;

type ArticlesListingProps = {
  items: ContentSummaryDto[];
};

export function ArticlesListing({ items }: ArticlesListingProps) {
  const router = useRouter();
  const [query, setQuery] = useState("");
  const [type, setType] = useState("all");
  const [page, setPage] = useState(1);

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase();
    return items.filter((item) => {
      if (type !== "all" && item.type.toLowerCase() !== type.toLowerCase()) return false;
      if (!q) return true;
      return `${item.title} ${item.slug}`.toLowerCase().includes(q);
    });
  }, [items, query, type]);

  const featured = filtered[0];
  const gridSource = filtered.slice(featured ? 1 : 0);
  const totalPages = Math.max(1, Math.ceil(gridSource.length / PAGE_SIZE));
  const safePage = Math.min(page, totalPages);
  const pageItems = gridSource.slice((safePage - 1) * PAGE_SIZE, safePage * PAGE_SIZE);

  return (
    <div dir="rtl">
      <PublicSection className="pb-6 pt-10 sm:pt-12" aria-labelledby="articles-hero-title">
        <PublicContainer>
          <PremiumBadge variant="ai" className="mb-3">
            Knowledge Base
          </PremiumBadge>
          <h1 id="articles-hero-title" className="text-3xl font-extrabold sm:text-4xl">
            <GradientText>مقالات مهندسی</GradientText>
          </h1>
          <p className="mt-2 max-w-2xl text-[14px] text-[color:var(--pub-muted)]">
            کارت‌های پریمیوم با دسته‌بندی، زمان مطالعه و فیلتر — داده از Content API
          </p>

          <div className="mt-6 flex flex-col gap-3 rounded-[var(--pub-radius)] border border-[color:var(--pub-glass-border)] bg-[color:var(--pub-glass)] p-3 backdrop-blur-md sm:flex-row sm:items-center">
            <label className="sr-only" htmlFor="articles-search">
              جستجو در مقالات
            </label>
            <input
              id="articles-search"
              type="search"
              value={query}
              onChange={(e) => {
                setQuery(e.target.value);
                setPage(1);
              }}
              onKeyDown={(e) => {
                if (e.key === "Enter" && query.trim()) {
                  router.push(`/search?q=${encodeURIComponent(query.trim())}&type=content`);
                }
              }}
              placeholder="جستجو در عنوان..."
              className="focus-ring h-11 flex-1 rounded-xl border border-[color:var(--pub-glass-border)] bg-[color:var(--pub-bg-elevated)] px-3 text-sm text-[color:var(--pub-fg)]"
            />
            <div className="flex flex-wrap gap-1.5" role="group" aria-label="فیلتر نوع">
              {TYPE_FILTERS.map((filter) => (
                <button
                  key={filter.value}
                  type="button"
                  aria-pressed={type === filter.value}
                  onClick={() => {
                    setType(filter.value);
                    setPage(1);
                  }}
                  className={[
                    "focus-ring rounded-lg px-3 py-2 text-[12px] font-semibold",
                    type === filter.value
                      ? "bg-[color:color-mix(in_srgb,var(--pub-primary)_20%,transparent)] text-[color:var(--pub-ai-from)]"
                      : "text-[color:var(--pub-muted)] hover:bg-white/[0.04]",
                  ].join(" ")}
                >
                  {filter.label}
                </button>
              ))}
            </div>
          </div>
        </PublicContainer>
      </PublicSection>

      <PublicSection className="pt-0" bare>
        <PublicContainer className="space-y-6">
          {filtered.length === 0 ? (
            <PageEmptyState
              title="مقاله‌ای یافت نشد"
              description={
                items.length === 0
                  ? "هنوز Article/News منتشر نشده است."
                  : "فیلتر یا عبارت جستجو را تغییر دهید."
              }
            />
          ) : (
            <>
              {featured && safePage === 1 ? (
                <KnowledgeCard
                  featured
                  title={featured.title}
                  href={publicHrefForContent(featured)}
                  category={labelForContentType(featured.type)}
                  readingTime={estimateReadingLabel(featured.title)}
                  difficulty={softDifficulty(featured.type)}
                  author={formatDateFa(featured.createdAt)}
                  coverTone="indigo"
                  className="pub-fade-up"
                />
              ) : null}

              <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                {pageItems.map((item, index) => (
                  <KnowledgeCard
                    key={item.id}
                    title={item.title}
                    href={publicHrefForContent(item)}
                    category={labelForContentType(item.type)}
                    readingTime={estimateReadingLabel(item.title)}
                    difficulty={softDifficulty(item.type)}
                    author={formatDateFa(item.createdAt)}
                    coverTone={index % 3 === 0 ? "violet" : index % 3 === 1 ? "cyan" : "indigo"}
                  />
                ))}
              </div>

              {gridSource.length > PAGE_SIZE ? (
                <nav className="flex items-center justify-center gap-3 pt-2" aria-label="صفحه‌بندی">
                  <GlowButton
                    variant="secondary"
                    disabled={safePage <= 1}
                    onClick={() => setPage((p) => Math.max(1, p - 1))}
                    aria-label="صفحه قبل"
                    className="!px-4 !py-2"
                  >
                    قبلی
                  </GlowButton>
                  <span className="text-[12px] text-[color:var(--pub-muted)]" aria-live="polite">
                    {safePage.toLocaleString("fa-IR")} / {totalPages.toLocaleString("fa-IR")}
                  </span>
                  <GlowButton
                    variant="secondary"
                    disabled={safePage >= totalPages}
                    onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                    aria-label="صفحه بعد"
                    className="!px-4 !py-2"
                  >
                    بعدی
                  </GlowButton>
                </nav>
              ) : null}
            </>
          )}
        </PublicContainer>
      </PublicSection>
    </div>
  );
}
