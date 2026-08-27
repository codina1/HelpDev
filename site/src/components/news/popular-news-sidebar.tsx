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

function EyeIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path d="M2.5 12s3.5-6.5 9.5-6.5S21.5 12 21.5 12s-3.5 6.5-9.5 6.5S2.5 12 2.5 12Z" stroke="currentColor" strokeWidth="1.7" strokeLinejoin="round" />
      <circle cx="12" cy="12" r="2.6" stroke="currentColor" strokeWidth="1.7" />
    </svg>
  );
}

/** Popular list — rank + content + thumbnail (RTL reference). */
export function PopularNewsSidebar() {
  return (
    <section
      className="rounded-[16px] border border-white/[0.08] bg-[#0F172A] p-4 shadow-[0_8px_28px_rgba(2,6,23,0.3)]"
      aria-labelledby="popular-news-heading"
      dir="rtl"
    >
      <h2 id="popular-news-heading" className="flex items-center gap-2 text-[15px] font-extrabold text-white">
        <FlameIcon className="h-4 w-4 text-[#F97316]" />
        محبوب‌ترین اخبار
      </h2>

      <ol className="mt-4 space-y-2.5">
        {NEWS_POPULAR.map((item, index) => (
          <li key={item.id}>
            <Link
              href={`#news-${item.id}`}
              className="group flex items-center gap-2.5 rounded-xl border border-white/[0.06] bg-[#111827]/80 p-2 no-underline transition hover:border-[rgba(168,85,247,0.35)]"
            >
              <span
                className={[
                  "flex h-7 w-7 shrink-0 items-center justify-center rounded-md text-[12px] font-extrabold",
                  index < 2
                    ? "bg-[#7C3AED] text-white shadow-[0_0_12px_rgba(124,58,237,0.45)]"
                    : "border border-white/[0.1] bg-[#1E293B] text-[#CBD5E1]",
                ].join(" ")}
              >
                {index + 1}
              </span>

              <span className="min-w-0 flex-1">
                <span className="line-clamp-1 text-[12px] font-extrabold text-white group-hover:text-[#E9D5FF]">
                  {item.title}
                </span>
                <span className="mt-0.5 line-clamp-1 text-[10px] text-[#94A3B8]">{item.summary}</span>
                <span className="mt-1 inline-flex items-center gap-1 text-[10px] font-semibold text-[#64748B]">
                  <EyeIcon className="h-3 w-3 text-[#A78BFA]" />
                  {item.views}
                </span>
              </span>

              <span className="h-11 w-11 shrink-0 overflow-hidden rounded-lg border border-white/[0.08] bg-[#080d1c]">
                <img
                  src={item.image}
                  alt=""
                  width={44}
                  height={44}
                  decoding="async"
                  className="h-full w-full object-cover"
                />
              </span>
            </Link>
          </li>
        ))}
      </ol>
    </section>
  );
}
