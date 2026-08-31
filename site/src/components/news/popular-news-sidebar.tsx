import Link from "next/link";
import { NEWS_POPULAR } from "@/data/news-articles";

function FlameIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M12 3c1.8 2.2 3 4.2 3 6.2 0 1.2-.4 2.2-1 3 .8-.3 1.5-1 1.9-1.9.6 1.1.9 2.3.9 3.5A5.8 5.8 0 0 1 12 21a5.8 5.8 0 0 1-4.8-9.1C8.4 9.8 10 6.8 12 3Z"
        stroke="currentColor"
        strokeWidth="1.7"
        strokeLinejoin="round"
      />
    </svg>
  );
}

function ClockIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <circle cx="12" cy="12" r="8" stroke="currentColor" strokeWidth="1.8" />
      <path d="M12 8v4.2l2.5 1.5" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  );
}

function EyeIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path d="M2.5 12s3.5-6.5 9.5-6.5S21.5 12 21.5 12s-3.5 6.5-9.5 6.5S2.5 12 2.5 12Z" stroke="currentColor" strokeWidth="1.7" strokeLinejoin="round" />
      <circle cx="12" cy="12" r="2.6" stroke="currentColor" strokeWidth="1.7" />
    </svg>
  );
}

/** Popular list — compact rows: thumbnail · content · rank (RTL reference). */
export function PopularNewsSidebar() {
  return (
    <section
      className="rounded-[16px] border border-white/[0.07] bg-[#0B1120] p-3.5 shadow-[0_6px_20px_rgba(2,6,23,0.28)]"
      aria-labelledby="popular-news-heading"
      dir="rtl"
    >
      <h2
        id="popular-news-heading"
        className="flex items-center gap-2 px-1 text-[14px] font-extrabold text-white"
      >
        <FlameIcon className="h-4 w-4 shrink-0 text-[#F97316]" />
        محبوب‌ترین اخبار
      </h2>

      <ol className="mt-3.5 space-y-1">
        {NEWS_POPULAR.map((item, index) => (
          <li key={item.id}>
            <Link
              href={`#news-${item.id}`}
              className="group flex items-center gap-2.5 rounded-xl border border-transparent p-1.5 no-underline transition hover:border-white/[0.08] hover:bg-white/[0.03]"
            >
              <span className="order-3 h-[52px] w-[52px] shrink-0 overflow-hidden rounded-[10px] border border-white/[0.07] bg-[#080d1c]">
                <img
                  src={item.image}
                  alt=""
                  width={52}
                  height={52}
                  loading="lazy"
                  decoding="async"
                  className="h-full w-full object-cover"
                />
              </span>

              <span className="order-2 min-w-0 flex-1">
                <span className="line-clamp-2 text-[13px] font-bold leading-5 text-white transition group-hover:text-[#C4B5FD]">
                  {item.title}
                </span>
                <span className="mt-0.5 line-clamp-1 text-[10.5px] leading-4 text-[#8B98AC]">
                  {item.summary}
                </span>
                <span className="mt-1 flex items-center gap-2.5 text-[10px] font-semibold text-[#64748B]">
                  <span className="inline-flex items-center gap-1 whitespace-nowrap">
                    <ClockIcon className="h-3 w-3 shrink-0 text-[#7C3AED]" />
                    <bdi>{item.time}</bdi>
                  </span>
                  <span className="inline-flex items-center gap-1 whitespace-nowrap">
                    <EyeIcon className="h-3 w-3 shrink-0 text-[#A78BFA]" />
                    <bdi>{item.views}</bdi>
                  </span>
                </span>
              </span>

              <span
                className={[
                  "order-1 flex h-7 w-7 shrink-0 items-center justify-center rounded-lg text-[12px] font-extrabold",
                  index < 2
                    ? "bg-gradient-to-br from-[#7C3AED] to-[#3B82F6] text-white shadow-[0_0_10px_rgba(124,58,237,0.3)]"
                    : "border border-white/[0.09] bg-[#131C31] text-[#94A3B8]",
                ].join(" ")}
              >
                {index + 1}
              </span>
            </Link>
          </li>
        ))}
      </ol>
    </section>
  );
}
