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
 * visual LEFT: TOC · related news · newsletter
 * CENTER: hero · body · tags · author
 * visual RIGHT: info · share · newsletter · course
 */
export function ArticleDetailView({ article }: ArticleDetailViewProps) {
  const model = buildArticleDetailViewModel(article);

  return (
    <div className="bg-[#050816] pb-12 pt-4">
      <ArticleReadingProgress />
      <ArticlesContainer>
        <div className="mb-5">
          <ArticleBreadcrumb items={model.breadcrumb} />
        </div>

        <div
          dir="ltr"
          className="grid grid-cols-1 items-start gap-6 xl:grid-cols-[280px_minmax(0,1fr)_280px] xl:gap-7"
        >
          <aside className="order-2 hidden xl:order-1 xl:sticky xl:top-20 xl:block xl:self-start" dir="rtl">
            <ArticleSidebarLeft headings={model.toc} relatedNews={model.relatedNews} />
          </aside>

          <article className="order-1 min-w-0 space-y-6 xl:order-2" dir="rtl">
            <ArticleHero model={model} />

            <div className="xl:hidden">
              <ArticleToc headings={model.toc} />
            </div>

            <ArticleContent
              usesBlocks={model.usesBlocks}
              contentHtml={model.contentHtml}
              contentBody={model.contentBody}
              headings={model.toc}
              tags={model.tags}
            />

            <ArticleAuthor author={model.author} />
          </article>

          <aside className="order-3 xl:sticky xl:top-20 xl:self-start" dir="rtl">
            <ArticleSidebarRight model={model} />
          </aside>
        </div>

        <div dir="rtl">
          <ArticleRelated
            articles={model.relatedArticles}
            course={model.relatedCourse}
            roadmap={model.roadmap}
          />
        </div>
      </ArticlesContainer>
    </div>
  );
}
