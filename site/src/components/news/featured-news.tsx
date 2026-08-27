import type { NewsArticle } from "@/types";

type FeaturedNewsProps = {
  article: NewsArticle;
};

export const NEWS_IMAGE_BY_TAG: Record<NewsArticle["tag"], string> = {
  React: "/home/icon-frontend.png",
  ".NET": "/home/icon-dotnet.png",
  AI: "/home/icon-ai.png",
  DevOps: "/home/icon-devops.png",
};

/** Large featured story card — image left / content right on desktop. */
export function FeaturedNews({ article }: FeaturedNewsProps) {
  return (
    <article
      id={`news-${article.id}`}
      className="group grid min-w-0 overflow-hidden rounded-[18px] border border-white/[0.08] bg-[#111827] shadow-[0_0_30px_rgba(2,6,23,0.25)] transition duration-500 ease-out hover:-translate-y-2 hover:border-[rgba(124,58,237,0.5)] hover:shadow-[0_18px_46px_rgba(2,6,23,0.55),0_0_36px_rgba(124,58,237,0.32)] sm:rounded-[20px] md:grid-cols-2 md:min-h-[280px] lg:min-h-[300px] xl:min-h-[330px]"
      dir="ltr"
    >
      <div className="relative aspect-[16/10] min-h-0 overflow-hidden bg-[#080d1c] sm:aspect-video md:aspect-auto md:h-full">
        <img
          src={NEWS_IMAGE_BY_TAG[article.tag]}
          alt=""
          width={640}
          height={360}
          decoding="async"
          className="h-full w-full object-contain p-4 transition duration-500 ease-out group-hover:scale-[1.03] sm:p-5"
        />
        <span
          className="pointer-events-none absolute inset-0 bg-gradient-to-t from-[#111827]/80 via-transparent to-transparent md:bg-gradient-to-r md:from-transparent md:to-[#111827]/40"
          aria-hidden
        />
        <span className="absolute end-3 top-3 rounded-full border border-[rgba(168,85,247,0.42)] bg-[rgba(76,29,149,0.72)] px-2.5 py-1 text-[10px] font-bold text-[#F3E8FF] backdrop-blur-md sm:end-4 sm:top-4 sm:px-3 sm:text-[11px]">
          {article.tag}
        </span>
      </div>

      <div
        className="flex flex-col justify-center p-4 text-right sm:p-6 md:p-6 lg:p-7 xl:p-8"
        dir="rtl"
      >
        <p className="mb-2 text-[11px] font-bold tracking-wide text-[#A78BFA] sm:mb-3 sm:text-[12px]">
          خبر ویژه
        </p>
        <h2 className="text-[18px] font-extrabold leading-8 text-white sm:text-[22px] sm:leading-9 lg:text-[24px] xl:text-[26px]">
          {article.title}
        </h2>
        <p className="mt-2 line-clamp-2 text-[13px] leading-6 text-[#94A3B8] sm:mt-3 sm:line-clamp-3 sm:text-[14px] sm:leading-7">
          {article.summary}
        </p>
        <time className="mt-4 text-[11px] font-semibold text-[#64748B] sm:mt-5 sm:text-[12px]">
          {article.time}
        </time>
      </div>
    </article>
  );
}
