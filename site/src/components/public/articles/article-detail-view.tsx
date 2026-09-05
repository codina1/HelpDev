import { ArticleAuthorCard, ArticleTagsCard } from "@/components/articles/detail/article-author-card";
import { ArticleBreadcrumb } from "@/components/articles/detail/article-breadcrumb";
import { ArticleRelatedList } from "@/components/articles/detail/article-related-list";
import {
  ArticleRelatedCourseCard,
  ArticleRelatedTools,
  ArticleRoadmapCtaCard,
} from "@/components/articles/detail/article-side-widgets";
import { ArticleTocNav } from "@/components/articles/detail/article-toc-nav";
import { ArticlesContainer } from "@/components/articles/articles-container";
import { ArticleHtmlBody } from "@/components/public/articles/article-html-body";
import {
  formatViewsShort,
  getArticleRelatedCourse,
  getArticleRelatedTools,
  getArticleRoadmapCta,
  resolveArticleAuthor,
  resolveArticleCategoryLabel,
  resolveArticleExcerpt,
  resolveArticleTags,
  resolveBreadcrumbTrail,
  resolveRelatedArticles,
} from "@/data/article-detail";
import type { ContentDetailDto } from "@/lib/api/content";
import {
  formatDateFa,
  resolveContentCoverUrl,
  shortAuthorId,
} from "@/lib/admin/content/content-mappers";
import { estimateReadingLabel } from "@/lib/public/display-meta";
import { extractTocFromBody, extractTocFromHtml, isBlockArticle } from "@/lib/public/content-helpers";

type ArticleDetailViewProps = {
  article: ContentDetailDto;
};

function EyeIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path d="M2.5 12s3.5-6.5 9.5-6.5S21.5 12 21.5 12s-3.5 6.5-9.5 6.5S2.5 12 2.5 12Z" stroke="currentColor" strokeWidth="1.6" />
      <circle cx="12" cy="12" r="2.6" stroke="currentColor" strokeWidth="1.6" />
    </svg>
  );
}

function ClockIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <circle cx="12" cy="12" r="8" stroke="currentColor" strokeWidth="1.6" />
      <path d="M12 8v4.5l3 1.8" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" />
    </svg>
  );
}

function CalendarIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <rect x="3.5" y="5" width="17" height="15" rx="2" stroke="currentColor" strokeWidth="1.6" />
      <path d="M8 3.5v3M16 3.5v3M3.5 9.5h17" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" />
    </svg>
  );
}

/**
 * Article detail — 3 columns:
 * visual LEFT: TOC + related + roadmap CTA
 * CENTER: hero + header + body
 * visual RIGHT: author + tags + tools + course
 */
export function ArticleDetailView({ article }: ArticleDetailViewProps) {
  const usesBlocks = isBlockArticle(article.contentFormat, article.contentHtml);
  const toc = usesBlocks
    ? extractTocFromHtml(article.contentHtml ?? "")
    : extractTocFromBody(article.body ?? "");

  const readingMinutes = article.readingTimeMinutes;
  const readingLabel = readingMinutes
    ? `${readingMinutes.toLocaleString("fa-IR")} دقیقه مطالعه`
    : estimateReadingLabel(article.title);

  const coverUrl = resolveContentCoverUrl(article.coverImage);
  const author = resolveArticleAuthor(article);
  const category = resolveArticleCategoryLabel(article);
  const excerpt = resolveArticleExcerpt(article);
  const tags = resolveArticleTags(article);
  const related = resolveRelatedArticles(article.slug);
  const crumbs = resolveBreadcrumbTrail(article);
  const displayAuthor = author.name || shortAuthorId(article.authorId);

  return (
    <div className="bg-[#050816] pb-10 pt-4">
      <ArticlesContainer>
        <div className="mb-5">
          <ArticleBreadcrumb items={crumbs} />
        </div>

        {/* LTR grid → TOC left · content center · author right */}
        <div
          dir="ltr"
          className="grid grid-cols-1 items-start gap-6 xl:grid-cols-[260px_minmax(0,1fr)_260px] xl:gap-6"
        >
          {/* LEFT rail */}
          <aside className="order-2 space-y-4 xl:order-1 xl:sticky xl:top-20 xl:self-start" dir="rtl">
            <div className="hidden xl:block">
              <ArticleTocNav headings={toc} />
            </div>
            <ArticleRelatedList articles={related} />
            <ArticleRoadmapCtaCard cta={getArticleRoadmapCta()} />
          </aside>

          {/* CENTER */}
          <article className="order-1 min-w-0 xl:order-2" dir="rtl">
            <div className="overflow-hidden rounded-[20px] border border-white/[0.08] bg-[#080D1F]/40 shadow-[0_0_40px_rgba(124,58,237,0.12)]">
              {coverUrl ? (
                <div className="relative aspect-[16/7] w-full overflow-hidden bg-[#080D1F]">
                  <img
                    src={coverUrl}
                    alt=""
                    className="h-full w-full object-cover mix-blend-screen"
                  />
                  <span
                    className="pointer-events-none absolute inset-0 bg-[radial-gradient(circle_at_70%_40%,rgba(139,92,246,0.28),transparent_55%)]"
                    aria-hidden
                  />
                </div>
              ) : (
                <div
                  className="relative flex aspect-[16/7] items-center justify-center overflow-hidden bg-gradient-to-bl from-[#8B5CF6]/35 via-[#2563EB]/15 to-transparent"
                  aria-hidden
                >
                  <span className="text-[28px] font-extrabold tracking-tight text-white/90 sm:text-[36px]">
                    HelpDev
                  </span>
                  <span className="pointer-events-none absolute inset-0 bg-[radial-gradient(circle_at_60%_45%,rgba(37,99,235,0.25),transparent_60%)]" />
                </div>
              )}
            </div>

            <header className="mt-5 space-y-3">
              <span className="inline-flex items-center rounded-lg border border-[#2563EB]/35 bg-[#2563EB]/15 px-2.5 py-1 text-[11.5px] font-bold text-[#93C5FD]">
                {category}
              </span>

              <h1 className="text-[28px] font-extrabold leading-[1.35] tracking-tight text-white sm:text-[36px] lg:text-[42px] xl:text-[48px] xl:leading-[1.25]">
                {article.title}
              </h1>

              <p className="max-w-3xl text-[15px] leading-7 text-[#94A3B8] sm:text-[17px] sm:leading-8 lg:text-[18px]">
                {excerpt}
              </p>

              <div className="flex flex-wrap items-center gap-x-4 gap-y-2 border-y border-white/[0.08] py-3 text-[12.5px] font-semibold text-[#94A3B8]">
                <span className="inline-flex items-center gap-2">
                  <span className="inline-flex h-7 w-7 items-center justify-center rounded-full border border-white/[0.1] bg-gradient-to-br from-[#8B5CF6]/45 to-[#2563EB]/25 text-[10px] font-bold text-white">
                    {author.initials}
                  </span>
                  {displayAuthor}
                </span>
                <span className="inline-flex items-center gap-1.5">
                  <CalendarIcon className="h-3.5 w-3.5" />
                  {formatDateFa(article.createdAt) || "—"}
                </span>
                <span className="inline-flex items-center gap-1.5">
                  <EyeIcon className="h-3.5 w-3.5" />
                  {formatViewsShort(article.views)} بازدید
                </span>
                <span className="inline-flex items-center gap-1.5">
                  <ClockIcon className="h-3.5 w-3.5" />
                  {readingLabel}
                </span>
              </div>
            </header>

            <div className="mt-5 xl:hidden">
              <ArticleTocNav headings={toc} />
            </div>

            <div className="mt-6 rounded-xl border border-white/[0.08] bg-[#080D1F]/70 p-5 shadow-[0_0_28px_rgba(2,6,23,0.35)] backdrop-blur-xl sm:p-7">
              {usesBlocks ? (
                <ArticleHtmlBody html={article.contentHtml ?? ""} />
              ) : (
                <ReadingBody body={article.body ?? ""} headings={toc} />
              )}
            </div>
          </article>

          {/* RIGHT rail */}
          <aside className="order-3 space-y-4 xl:sticky xl:top-20 xl:self-start" dir="rtl">
            <ArticleAuthorCard author={author} />
            <ArticleTagsCard tags={tags} />
            <ArticleRelatedTools tools={getArticleRelatedTools()} />
            <ArticleRelatedCourseCard course={getArticleRelatedCourse()} />
          </aside>
        </div>
      </ArticlesContainer>
    </div>
  );
}

function ReadingBody({
  body,
  headings,
}: {
  body: string;
  headings: ReturnType<typeof extractTocFromBody>;
}) {
  const blocks = body.split(/\r?\n/);
  let headingIndex = 0;

  return (
    <div className="space-y-4 text-[17px] leading-8 text-[#E2E8F0] sm:text-[18px] sm:leading-9">
      {blocks.map((line, index) => {
        const headingMatch = /^(#{1,3})\s+(.+?)\s*$/.exec(line.trim());
        if (headingMatch) {
          const level = headingMatch[1].length;
          const text = headingMatch[2].replace(/\{#[^}]+\}\s*$/, "").trim();
          const id =
            level >= 2 && headingIndex < headings.length ? headings[headingIndex++].id : undefined;
          if (level >= 3) {
            return (
              <h3
                key={index}
                id={id}
                className="scroll-mt-28 border-e-[3px] border-[#8B5CF6] pe-3 pt-3 text-[20px] font-extrabold text-white sm:text-[22px]"
              >
                {text}
              </h3>
            );
          }
          return (
            <h2
              key={index}
              id={id}
              className="scroll-mt-28 border-e-[3px] border-[#8B5CF6] pe-3 pt-4 text-[24px] font-extrabold text-white sm:text-[28px]"
            >
              {text}
            </h2>
          );
        }
        if (!line.trim()) return <div key={index} className="h-2" aria-hidden />;
        return (
          <p key={index} className="text-[#94A3B8]">
            {line}
          </p>
        );
      })}
      {!body.trim() ? (
        <p className="rounded-xl border border-dashed border-white/[0.12] px-4 py-8 text-center text-sm text-[#94A3B8]">
          بدنه این محتوا خالی است.
        </p>
      ) : null}
    </div>
  );
}
