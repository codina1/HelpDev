import type { NewsArticle } from "@/types";

type NewsArticleCardProps = {
  article: NewsArticle;
};

function ClockIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <circle cx="12" cy="12" r="8" stroke="currentColor" strokeWidth="1.8" />
      <path d="M12 8v4.2l2.5 1.5" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  );
}

function BookIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path d="M5 6.5A2.5 2.5 0 0 1 7.5 4H19v14.5H7.5A2.5 2.5 0 0 0 5 21V6.5Z" stroke="currentColor" strokeWidth="1.8" strokeLinejoin="round" />
      <path d="M5 18.5h14" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  );
}

export function NewsArticleCard({ article }: NewsArticleCardProps) {
  return (
    <article
      id={`news-${article.id}`}
      className="group flex h-full min-w-0 flex-col overflow-hidden rounded-[16px] border border-white/[0.08] bg-[#111827] shadow-[0_8px_28px_rgba(2,6,23,0.3)] transition duration-300 hover:-translate-y-1 hover:border-[rgba(124,58,237,0.45)]"
    >
      <div className="relative aspect-[16/10] w-full shrink-0 overflow-hidden bg-[#080d1c]">
        <img
          src={article.image}
          alt=""
          width={400}
          height={250}
          decoding="async"
          className="absolute inset-0 h-full w-full object-cover object-center transition duration-500 group-hover:scale-[1.04]"
        />
        <span className="absolute end-3 top-3 rounded-lg bg-[#7C3AED] px-2.5 py-1 text-[11px] font-bold text-white">
          {article.categoryLabel ?? article.tag}
        </span>
      </div>
      <div className="flex flex-1 flex-col gap-2 p-4 sm:p-5">
        <h2 className="line-clamp-2 text-[15px] font-extrabold leading-7 text-white sm:text-[16px]">
          {article.title}
        </h2>
        <p className="line-clamp-2 text-[12px] leading-6 text-[#94A3B8] sm:text-[13px]">
          {article.summary}
        </p>
        <div className="mt-auto flex flex-wrap items-center gap-x-3 gap-y-1.5 pt-2 text-[11px] font-semibold text-[#64748B] sm:text-[12px]">
          <span className="inline-flex items-center gap-1.5">
            <ClockIcon className="h-3.5 w-3.5 text-[#7C3AED]" />
            {article.time}
          </span>
          <span className="inline-flex items-center gap-1.5">
            <BookIcon className="h-3.5 w-3.5 text-[#7C3AED]" />
            {article.readTime}
          </span>
        </div>
      </div>
    </article>
  );
}
