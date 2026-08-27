import type { NewsArticle } from "@/types";
import { NEWS_IMAGE_BY_TAG } from "@/components/news/featured-news";

type NewsArticleCardProps = {
  article: NewsArticle;
};

export function NewsArticleCard({ article }: NewsArticleCardProps) {
  return (
    <article
      id={`news-${article.id}`}
      className="group flex h-full min-w-0 flex-col overflow-hidden rounded-[20px] border border-white/[0.08] bg-[#111827] shadow-[0_0_24px_rgba(2,6,23,0.22)] transition duration-500 ease-out hover:-translate-y-2 hover:border-[rgba(124,58,237,0.5)] hover:shadow-[0_18px_42px_rgba(2,6,23,0.5),0_0_34px_rgba(124,58,237,0.32)]"
    >
      <div className="relative aspect-video w-full shrink-0 overflow-hidden rounded-t-[20px] bg-[#080d1c]">
        <img
          src={NEWS_IMAGE_BY_TAG[article.tag]}
          alt=""
          width={640}
          height={440}
          decoding="async"
          className="h-full w-full object-contain transition duration-500 ease-out group-hover:scale-[1.02]"
        />
        <span
          className="pointer-events-none absolute inset-0 bg-gradient-to-t from-[#111827] via-transparent to-transparent"
          aria-hidden
        />
        <span className="absolute end-3 top-3 rounded-full border border-[rgba(168,85,247,0.4)] bg-[rgba(11,18,36,0.78)] px-3 py-1 text-[11px] font-bold text-[#E9D5FF] backdrop-blur-md">
          {article.tag}
        </span>
      </div>
      <div className="flex flex-1 flex-col p-4 sm:p-5">
        <h2 className="text-[18px] font-extrabold leading-7 text-white">{article.title}</h2>
        <p className="mt-1.5 line-clamp-2 text-[13px] leading-6 text-[#94A3B8]">{article.summary}</p>
        <time className="mt-auto pt-3 text-[12px] font-semibold text-[#64748B]">{article.time}</time>
      </div>
    </article>
  );
}
