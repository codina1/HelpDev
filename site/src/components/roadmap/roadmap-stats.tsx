import { ROADMAP_STATS } from "@/data/roadmap-paths";
import { RoadmapContainer } from "@/components/roadmap/roadmap-container";

function StatIcon({ name, className }: { name: string; className?: string }) {
  switch (name) {
    case "clock":
      return (
        <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
          <circle cx="12" cy="12" r="8.4" stroke="currentColor" strokeWidth="1.7" />
          <path d="M12 7.4V12l3 1.8" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" />
        </svg>
      );
    case "doc":
      return (
        <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M6.5 3.5h7.2L18.5 8v12.5h-12V3.5Z"
            stroke="currentColor"
            strokeWidth="1.7"
            strokeLinejoin="round"
          />
          <path
            d="M13.4 3.6V8.3h4.8M9.2 12.4h6M9.2 15.8h4"
            stroke="currentColor"
            strokeWidth="1.7"
            strokeLinecap="round"
          />
        </svg>
      );
    case "map":
      return (
        <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M3.6 6.4 9 4.2l6 2.2 5.4-2.2v13.4L15 19.8l-6-2.2-5.4 2.2V6.4Z"
            stroke="currentColor"
            strokeWidth="1.7"
            strokeLinejoin="round"
          />
          <path d="M9 4.2v13.4M15 6.4v13.4" stroke="currentColor" strokeWidth="1.7" />
        </svg>
      );
    default:
      return (
        <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
          <circle cx="9.4" cy="8.6" r="3.4" stroke="currentColor" strokeWidth="1.7" />
          <path
            d="M3.6 19.2c.6-3 3-4.8 5.8-4.8s5.2 1.8 5.8 4.8"
            stroke="currentColor"
            strokeWidth="1.7"
            strokeLinecap="round"
          />
          <path
            d="M16.4 6.2a3 3 0 0 1 0 5.6M17.6 14.9c2 .7 3.3 2.3 3.7 4.3"
            stroke="currentColor"
            strokeWidth="1.7"
            strokeLinecap="round"
          />
        </svg>
      );
  }
}

/** Four equal stat cells with purple values and hairline dividers. */
export function RoadmapStats() {
  return (
    <section className="bg-[#030713] pb-2.5 pt-1" dir="rtl" aria-label="آمار مسیرهای یادگیری">
      <RoadmapContainer>
        <div className="grid grid-cols-2 overflow-hidden rounded-[16px] border border-white/[0.08] bg-[#080D1E] md:grid-cols-4 md:divide-x md:divide-x-reverse md:divide-white/[0.07]">
          {ROADMAP_STATS.map((stat, index) => (
            <div
              key={stat.id}
              className={[
                "flex items-center justify-center gap-3 px-3 py-4 md:h-[88px] md:gap-4 md:px-5 md:py-0",
                index === 1 ? "border-s border-white/[0.07] md:border-s-0" : "",
                index > 1 ? "border-t border-white/[0.07] md:border-t-0" : "",
                index === 3 ? "border-s border-white/[0.07] md:border-s-0" : "",
              ].join(" ")}
            >
              <div className="min-w-0 text-center">
                <p className="text-[24px] font-extrabold leading-none text-[#A855F7] sm:text-[28px] md:text-[30px]">
                  {stat.value}
                </p>
                <p className="mt-1.5 text-[12px] font-semibold text-[#94A3B8]">{stat.label}</p>
              </div>
              <span
                className="inline-flex h-11 w-11 shrink-0 items-center justify-center rounded-xl border border-[rgba(124,58,237,0.4)] bg-[rgba(124,58,237,0.14)] text-[#C4B5FD] shadow-[0_0_16px_rgba(124,58,237,0.2)]"
                aria-hidden
              >
                <StatIcon name={stat.icon} className="h-5 w-5" />
              </span>
            </div>
          ))}
        </div>
      </RoadmapContainer>
    </section>
  );
}
