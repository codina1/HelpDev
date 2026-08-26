import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { PublicSection } from "@/components/ui/public/v2/public-section";

export const HOME_PLATFORM_STATS = [
  { id: "articles", value: "+۱۲۰۰", label: "مقاله آموزشی", tone: "blue" as const },
  { id: "prompts", value: "+۵۰۰", label: "Prompt آماده", tone: "purple" as const },
  { id: "tools", value: "+۸۰", label: "ابزار کاربردی", tone: "cyan" as const },
  { id: "roadmaps", value: "+۶۰", label: "نقشه راه", tone: "blue" as const },
  { id: "devs", value: "+۲۵K", label: "توسعه‌دهنده", tone: "purple" as const },
] as const;

const TONE: Record<(typeof HOME_PLATFORM_STATS)[number]["tone"], string> = {
  purple: "bg-[rgba(124,58,237,0.16)] text-[#C4B5FD]",
  blue: "bg-[rgba(99,102,241,0.16)] text-[#A5B4FC]",
  cyan: "bg-[rgba(6,182,212,0.14)] text-[#67E8F9]",
};

/** Horizontal stats row with reference line icons. Mobile: 2-col grid. */
export function HomeStatsSection() {
  return (
    <PublicSection className="home-platform-stats home-reveal" bare aria-label="آمار پلتفرم">
      <PublicContainer size="wide">
        <dl className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-5 sm:gap-0 sm:divide-x sm:divide-x-reverse sm:divide-white/[0.08] sm:rounded-2xl sm:border sm:border-white/[0.08] sm:bg-[#0B1224]/70 sm:backdrop-blur-md">
          {HOME_PLATFORM_STATS.map((stat) => (
            <div
              key={stat.id}
              className="flex items-center gap-3 rounded-2xl border border-white/[0.08] bg-[#0B1224] px-3 py-3.5 sm:justify-center sm:rounded-none sm:border-0 sm:bg-transparent sm:px-4 sm:py-5"
            >
              <span
                className={`flex h-9 w-9 shrink-0 items-center justify-center rounded-xl ${TONE[stat.tone]}`}
                aria-hidden
              >
                <StatIcon id={stat.id} />
              </span>
              <div className="min-w-0 text-start">
                <dt className="sr-only">{stat.label}</dt>
                <dd className="text-[1.05rem] font-extrabold tracking-tight text-white sm:text-xl">
                  {stat.value}
                </dd>
                <p className="mt-0.5 text-[11px] font-semibold text-[#94A3B8] sm:text-[12px]">
                  {stat.label}
                </p>
              </div>
            </div>
          ))}
        </dl>
      </PublicContainer>
    </PublicSection>
  );
}

function StatIcon({ id }: { id: string }) {
  const common = {
    width: 16,
    height: 16,
    viewBox: "0 0 24 24",
    fill: "none",
    stroke: "currentColor",
    strokeWidth: 1.6,
    strokeLinecap: "round" as const,
    strokeLinejoin: "round" as const,
  };

  if (id === "articles") {
    return (
      <svg {...common} aria-hidden>
        <rect x="3" y="3" width="18" height="18" rx="2.5" />
        <path d="M3 9.5h18M3 15h18M9.5 3v18M15 3v18" />
      </svg>
    );
  }
  if (id === "prompts") {
    return (
      <svg {...common} aria-hidden>
        <path d="M7 4h10a2 2 0 0 1 2 2v14l-7-3.2L5 20V6a2 2 0 0 1 2-2Z" />
        <circle cx="12" cy="10" r="2.2" />
        <path d="M8.8 15.2c.9-1.1 2-1.7 3.2-1.7s2.3.6 3.2 1.7" />
      </svg>
    );
  }
  if (id === "tools") {
    return (
      <svg {...common} aria-hidden>
        <path d="M14.5 4.5a3.5 3.5 0 0 0-4.7 4.7L4 15l2.8 2.8 5.8-5.8a3.5 3.5 0 0 0 4.7-4.7L14.8 9l-1.6-1.6 1.3-2.9Z" />
        <path d="M16.2 14.2 20 18l-2.2 2.2-3.8-3.8" />
      </svg>
    );
  }
  if (id === "roadmaps") {
    return (
      <svg {...common} aria-hidden>
        <circle cx="6" cy="7" r="2.2" />
        <circle cx="18" cy="7" r="2.2" />
        <circle cx="12" cy="17" r="2.2" />
        <path d="M7.8 8.2 10.4 15M16.2 8.2 13.6 15M8.2 7h7.6" />
      </svg>
    );
  }
  return (
    <svg {...common} aria-hidden>
      <circle cx="12" cy="8" r="3.2" />
      <path d="M5.5 19.5c1.4-3.2 3.6-4.8 6.5-4.8s5.1 1.6 6.5 4.8" />
    </svg>
  );
}
