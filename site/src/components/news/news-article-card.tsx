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
      <path
        d="M5 6.5A2.5 2.5 0 0 1 7.5 4H19v14.5H7.5A2.5 2.5 0 0 0 5 21V6.5Z"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinejoin="round"
      />
      <path d="M5 18.5h14" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  );
}

export function NewsArticleCard({ article }: NewsArticleCardProps) {
  return (
    <article
      id={`news-${article.id}`}
      className="group flex h-full min-w-0 flex-col overflow-hidden rounded-[18px] border border-white/[0.08] bg-[#111827] shadow-[0_0_24px_rgba(2,6,23,0.22)] transition duration-500 ease-out hover:-translate-y-2 hover:border-[rgba(124,58,237,0.5)] hover:shadow-[0_18px_42px_rgba(2,6,23,0.5),0_0_34px_rgba(124,58,237,0.32)] sm:rounded-[20px]"
    >
      <div className="relative aspect-[16/10] w-full shrink-0 overflow-hidden rounded-t-[18px] bg-[#080d1c] sm:aspect-video sm:rounded-t-[20px]">
        <div
          className="pointer-events-none absolute inset-0 bg-[radial-gradient(circle_at_35%_30%,rgba(124,58,237,0.32),transparent_55%)]"
          aria-hidden
        />
        <img
          src={article.image}
          alt=""
          width={640}
          height={360}
          decoding="async"
          className="absolute inset-0 h-full w-full scale-110 object-cover object-center opacity-90 transition duration-500 ease-out group-hover:scale-[1.15]"
        />
        <span
          className="pointer-events-none absolute inset-0 bg-gradient-to-t from-[#111827] via-[#111827]/25 to-transparent"
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
        <div className="mt-auto flex flex-wrap items-center gap-x-3 gap-y-1.5 pt-2.5 text-[11px] font-semibold text-[#64748B] sm:pt-3 sm:text-[12px]">
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
