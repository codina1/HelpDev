import Link from "next/link";
import type { NewsArticle } from "@/types";

type PopularNewsSidebarProps = {
  articles: NewsArticle[];
};

/** Compact ranked list for the news page sidebar. */
export function PopularNewsSidebar({ articles }: PopularNewsSidebarProps) {
  return (
    <section
      className="rounded-[20px] border border-white/[0.08] bg-[#111827]/80 p-5 backdrop-blur-xl"
      aria-labelledby="popular-news-heading"
    >
      <h2 id="popular-news-heading" className="text-[17px] font-extrabold text-white">
        محبوب‌ترین اخبار
      </h2>
      <ol className="mt-4 divide-y divide-white/[0.06]">
        {articles.slice(0, 5).map((article, index) => (
          <li key={article.id} className="py-3 first:pt-0 last:pb-0">
            <Link
              href={`#news-${article.id}`}
              className="group flex items-start gap-3 text-start no-underline"
            >
              <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-lg bg-[rgba(124,58,237,0.16)] text-[12px] font-extrabold text-[#C4B5FD] transition group-hover:bg-[rgba(124,58,237,0.3)] group-hover:text-white">
                {index + 1}
              </span>
              <span className="min-w-0">
                <span className="line-clamp-2 text-[13px] font-bold leading-6 text-[#E2E8F0] transition group-hover:text-white">
                  {article.title}
                </span>
                <span className="mt-1 block text-[11px] font-medium text-[#64748B]">{article.time}</span>
              </span>
            </Link>
          </li>
        ))}
      </ol>
    </section>
  );
}
