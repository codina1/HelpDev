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

/** Large featured story card for the magazine-style news layout. */
export function FeaturedNews({ article }: FeaturedNewsProps) {
  return (
    <article
      id={`news-${article.id}`}
      className="group grid min-w-0 overflow-hidden rounded-[20px] border border-white/[0.08] bg-[#111827] shadow-[0_0_30px_rgba(2,6,23,0.25)] transition duration-500 ease-out hover:-translate-y-2 hover:border-[rgba(124,58,237,0.5)] hover:shadow-[0_18px_46px_rgba(2,6,23,0.55),0_0_36px_rgba(124,58,237,0.32)] md:grid-cols-2"
    >
      <div className="relative aspect-video min-h-0 overflow-hidden bg-[#080d1c] md:aspect-auto md:min-h-[300px]">
        <img
          src={NEWS_IMAGE_BY_TAG[article.tag]}
          alt=""
          width={640}
          height={360}
          decoding="async"
          className="h-full w-full object-contain transition duration-500 ease-out group-hover:scale-[1.03]"
        />
        <span
          className="pointer-events-none absolute inset-0 bg-gradient-to-t from-[#111827]/80 via-transparent to-transparent"
          aria-hidden
        />
        <span className="absolute end-4 top-4 rounded-full border border-[rgba(168,85,247,0.42)] bg-[rgba(76,29,149,0.72)] px-3 py-1 text-[11px] font-bold text-[#F3E8FF] backdrop-blur-md">
          {article.tag}
        </span>
      </div>

      <div className="flex flex-col justify-center p-5 sm:p-7 lg:p-8">
        <p className="mb-3 text-[12px] font-bold tracking-wide text-[#A78BFA]">خبر ویژه</p>
        <h2 className="text-[22px] font-extrabold leading-9 text-white sm:text-[26px]">
          {article.title}
        </h2>
        <p className="mt-3 line-clamp-3 text-[14px] leading-7 text-[#94A3B8]">{article.summary}</p>
        <time className="mt-5 text-[12px] font-semibold text-[#64748B]">{article.time}</time>
      </div>
    </article>
  );
}
