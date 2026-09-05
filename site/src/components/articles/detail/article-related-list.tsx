import Link from "next/link";
import type { MarketplaceArticle } from "@/data/articles";

type ArticleRelatedListProps = {
  articles: MarketplaceArticle[];
};

export function ArticleRelatedList({ articles }: ArticleRelatedListProps) {
  if (articles.length === 0) return null;

  return (
    <section
      className="rounded-xl border border-white/[0.08] bg-[#080D1F]/85 p-4 backdrop-blur-xl"
      aria-labelledby="related-articles-title"
    >
      <h2 id="related-articles-title" className="mb-3 text-[13px] font-extrabold text-white">
        مقالات مرتبط
      </h2>
      <ul className="space-y-2.5">
        {articles.map((article) => (
          <li key={article.id}>
            <Link
              href={`/articles/${article.slug}`}
              className="group flex gap-2.5 rounded-xl border border-transparent p-1.5 no-underline transition hover:border-white/[0.08] hover:bg-white/[0.03]"
            >
              <span
                className={[
                  "relative h-12 w-12 shrink-0 overflow-hidden rounded-lg bg-gradient-to-br",
                  article.coverTone,
                ].join(" ")}
              >
                <img
                  src={article.coverImage}
                  alt=""
                  width={48}
                  height={48}
                  className="h-full w-full object-cover mix-blend-screen"
                  loading="lazy"
                />
              </span>
              <span className="min-w-0 flex-1">
                <span className="line-clamp-2 text-[12.5px] font-bold leading-5 text-[#E5E7EB] transition group-hover:text-[#E9D5FF]">
                  {article.title}
                </span>
                <span className="mt-1 block text-[11px] text-[#64748B]">
                  {article.readingMinutes.toLocaleString("fa-IR")} دقیقه مطالعه
                </span>
              </span>
            </Link>
          </li>
        ))}
      </ul>
    </section>
  );
}
