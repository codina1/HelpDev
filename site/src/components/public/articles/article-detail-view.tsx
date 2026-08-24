import { ArticleAiAssistantPanel } from "@/components/public/articles/article-ai-panel";
import { ArticleToc, RelatedContentPlaceholder } from "@/components/public/articles/article-toc";
import { GlassCard, PremiumBadge, PublicContainer } from "@/components/ui/public/v2";
import type { ContentDetailDto } from "@/lib/api/content";
import {
  formatDateFa,
  labelForContentType,
  resolveContentCoverUrl,
  shortAuthorId,
} from "@/lib/admin/content/content-mappers";
import { estimateReadingLabel, softDifficulty } from "@/lib/public/display-meta";
import { extractTocFromBody, extractTocFromHtml, isBlockArticle } from "@/lib/public/content-helpers";
import { ArticleHtmlBody } from "@/components/public/articles/article-html-body";

type ArticleDetailViewProps = {
  article: ContentDetailDto;
};

export function ArticleDetailView({ article }: ArticleDetailViewProps) {
  const usesBlocks = isBlockArticle(article.contentFormat, article.contentHtml);
  const toc = usesBlocks
    ? extractTocFromHtml(article.contentHtml ?? "")
    : extractTocFromBody(article.body ?? "");
  const readingLabel = article.readingTimeMinutes
    ? `${article.readingTimeMinutes.toLocaleString("fa-IR")} دقیقه مطالعه`
    : estimateReadingLabel(article.title);
  const coverUrl = resolveContentCoverUrl(article.coverImage);

  return (
    <PublicContainer className="py-8 lg:py-12">
      <article className="grid gap-8 lg:grid-cols-[minmax(0,1fr)_300px]" dir="rtl">
        <div className="min-w-0">
          <GlassCard elevate={false} strong className="mb-8 overflow-hidden p-0">
            {coverUrl ? (
              <img
                src={coverUrl}
                alt=""
                className="h-48 w-full object-cover sm:h-56"
              />
            ) : (
              <div
                className="h-36 bg-gradient-to-bl from-[color:var(--pub-primary)]/35 via-[color:var(--pub-primary-2)]/15 to-transparent sm:h-44"
                aria-hidden
              />
            )}
            <header className="space-y-4 p-5 sm:p-7">
              <div className="flex flex-wrap gap-2">
                <PremiumBadge variant="primary">{labelForContentType(article.type)}</PremiumBadge>
                <PremiumBadge variant="outline">{softDifficulty(article.type)}</PremiumBadge>
                <PremiumBadge variant="muted">{readingLabel}</PremiumBadge>
              </div>
              <h1 className="text-2xl font-extrabold leading-10 text-[color:var(--pub-fg)] sm:text-3xl lg:text-4xl">
                {article.title}
              </h1>
              <dl className="flex flex-wrap gap-x-4 gap-y-1 text-[12px] text-[color:var(--pub-muted)]">
                <div className="flex gap-1">
                  <dt>تاریخ</dt>
                  <dd>{formatDateFa(article.createdAt) || "—"}</dd>
                </div>
                <div className="flex gap-1">
                  <dt>نویسنده</dt>
                  <dd>{shortAuthorId(article.authorId)}</dd>
                </div>
                <div className="flex gap-1">
                  <dt>بازدید</dt>
                  <dd>{article.views.toLocaleString("fa-IR")}</dd>
                </div>
              </dl>
            </header>
          </GlassCard>

          <div className="mb-6 lg:hidden">
            <ArticleToc headings={toc} />
          </div>

          <GlassCard elevate={false} className="p-5 sm:p-7">
            {usesBlocks ? (
              <ArticleHtmlBody html={article.contentHtml ?? ""} />
            ) : (
              <ReadingBody body={article.body ?? ""} headings={toc} />
            )}
          </GlassCard>
        </div>

        <aside className="space-y-4 lg:sticky lg:top-20 lg:self-start">
          <div className="hidden lg:block">
            <ArticleToc headings={toc} />
          </div>
          <ArticleAiAssistantPanel title={article.title} slug={article.slug} />
          <RelatedContentPlaceholder currentSlug={article.slug} />
        </aside>
      </article>
    </PublicContainer>
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
    <div className="space-y-4 text-[15px] leading-8 text-[color:var(--pub-fg)]/90">
      {blocks.map((line, index) => {
        const headingMatch = /^(#{1,3})\s+(.+?)\s*$/.exec(line.trim());
        if (headingMatch) {
          const level = headingMatch[1].length;
          const text = headingMatch[2].replace(/\{#[^}]+\}\s*$/, "").trim();
          const id =
            level >= 2 && headingIndex < headings.length ? headings[headingIndex++].id : undefined;
          const Tag = level >= 3 ? "h3" : "h2";
          return (
            <Tag
              key={index}
              id={id}
              className="scroll-mt-24 pt-2 text-xl font-extrabold text-[color:var(--pub-fg)]"
            >
              {text}
            </Tag>
          );
        }
        if (!line.trim()) return <div key={index} className="h-2" aria-hidden />;
        return (
          <p key={index} className="text-[color:var(--pub-muted)]">
            {line}
          </p>
        );
      })}
      {!body.trim() ? (
        <p className="rounded-xl border border-dashed border-[color:var(--pub-glass-border)] px-4 py-8 text-center text-sm text-[color:var(--pub-muted)]">
          بدنه این محتوا خالی است.
        </p>
      ) : null}
    </div>
  );
}
