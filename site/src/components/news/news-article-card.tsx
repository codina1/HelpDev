import type { NewsArticle } from "@/types";
import { NEWS_IMAGE_BY_TAG } from "@/components/news/featured-news";

type NewsArticleCardProps = {
  article: NewsArticle;
};

export function NewsArticleCard({ article }: NewsArticleCardProps) {
  return (
    <article
      id={`news-${article.id}`}
      className="group flex h-full min-w-0 flex-col overflow-hidden rounded-[18px] border border-white/[0.08] bg-[#111827] shadow-[0_0_24px_rgba(2,6,23,0.22)] transition duration-500 ease-out hover:-translate-y-2 hover:border-[rgba(124,58,237,0.5)] hover:shadow-[0_18px_42px_rgba(2,6,23,0.5),0_0_34px_rgba(124,58,237,0.32)] sm:rounded-[20px]"
    >
      <div className="relative aspect-[16/10] w-full shrink-0 overflow-hidden rounded-t-[18px] bg-[#080d1c] sm:aspect-video sm:rounded-t-[20px]">
        <img
          src={NEWS_IMAGE_BY_TAG[article.tag]}
          alt=""
          width={640}
          height={360}
          decoding="async"
          className="h-full w-full object-contain p-3 transition duration-500 ease-out group-hover:scale-[1.02] sm:p-4"
        />
        <span
          className="pointer-events-none absolute inset-0 bg-gradient-to-t from-[#111827] via-transparent to-transparent"
          aria-hidden
        />
        <span className="absolute end-2.5 top-2.5 rounded-full border border-[rgba(168,85,247,0.4)] bg-[rgba(11,18,36,0.78)] px-2.5 py-1 text-[10px] font-bold text-[#E9D5FF] backdrop-blur-md sm:end-3 sm:top-3 sm:text-[11px]">
          {article.tag}
        </span>
      </div>
      <div className="flex flex-1 flex-col p-3.5 sm:p-4 lg:p-5">
        <h2 className="line-clamp-2 text-[16px] font-extrabold leading-7 text-white sm:text-[17px] lg:text-[18px]">
          {article.title}
        </h2>
        <p className="mt-1.5 line-clamp-2 text-[12px] leading-6 text-[#94A3B8] sm:text-[13px]">
          {article.summary}
        </p>
        <time className="mt-auto pt-2.5 text-[11px] font-semibold text-[#64748B] sm:pt-3 sm:text-[12px]">
          {article.time}
        </time>
      </div>
    </article>
  );
}
