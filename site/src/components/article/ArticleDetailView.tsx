"use client";

import { ArticleAuthor } from "@/components/article/ArticleAuthor";
import { ArticleBreadcrumb } from "@/components/article/ArticleBreadcrumb";
import { ArticleContent } from "@/components/article/ArticleContent";
import { ArticleHero } from "@/components/article/ArticleHero";
import { ArticleReadingProgress } from "@/components/article/ArticleReadingProgress";
import {
  ArticleRelated,
  ArticleSidebarLeft,
  ArticleSidebarRight,
  ArticleToc,
} from "@/components/article/ArticleSidebar";
import { ArticlesContainer } from "@/components/articles/articles-container";
import { buildArticleDetailViewModel } from "@/data/article-detail";
import type { ContentDetailDto } from "@/lib/api/content";

type ArticleDetailViewProps = {
  article: ContentDetailDto;
};

/**
 * Premium HelpDev article / news detail — 3 columns:
 * visual LEFT: TOC · related · newsletter
 * CENTER: hero · body · tags · author
 * visual RIGHT: info · share · learning CTA
 */
export function ArticleDetailView({ article }: ArticleDetailViewProps) {
  const model = buildArticleDetailViewModel(article);

  return (
    <div className="bg-[#050816] pb-8 pt-3">
      <ArticleReadingProgress />
      <ArticlesContainer>
        <div className="mb-3">
          <ArticleBreadcrumb items={model.breadcrumb} />
        </div>

        <div
          dir="ltr"
          className="grid grid-cols-1 items-start gap-5 lg:grid-cols-[220px_minmax(0,1fr)] xl:grid-cols-[220px_minmax(0,1fr)_220px] xl:gap-6"
        >
          <aside className="order-2 hidden lg:order-1 lg:sticky lg:top-20 lg:block lg:self-start" dir="rtl">
            <ArticleSidebarLeft
              headings={model.toc}
              relatedNews={model.relatedNews}
              hub={model.hub}
            />
          </aside>

          <article className="order-1 min-w-0 space-y-5 xl:order-2" dir="rtl">
            <ArticleHero model={model} />

            <div className="lg:hidden">
              <ArticleToc headings={model.toc} hub={model.hub} collapsible />
            </div>

            <ArticleContent
              usesBlocks={model.usesBlocks}
              contentHtml={model.contentHtml}
              contentBody={model.contentBody}
              headings={model.toc}
              tags={model.tags}
              hub={model.hub}
            />

            <ArticleAuthor author={model.author} />
          </article>

          <aside className="order-3 xl:sticky xl:top-20 xl:self-start" dir="rtl">
            <ArticleSidebarRight model={model} />
          </aside>
        </div>

        <div dir="rtl">
          <ArticleRelated cards={model.relatedCards} />
        </div>
      </ArticlesContainer>
    </div>
  );
}
