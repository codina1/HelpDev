import type { NewsArticle } from "@/types";

type NewsArticleCardProps = {
  article: NewsArticle;
};

export function NewsArticleCard({ article }: NewsArticleCardProps) {
  return (
    <article className="ui-card p-5 sm:p-6">
      <div className="mb-3 flex flex-wrap items-center gap-2.5">
        <span className="ui-badge">{article.tag}</span>
        <time className="ui-meta">{article.time}</time>
      </div>
      <h2 className="ui-heading text-base">{article.title}</h2>
      <p className="ui-body mt-2">{article.summary}</p>
    </article>
  );
}
