import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { PublicSection } from "@/components/ui/public/v2/public-section";

export const HOME_PLATFORM_STATS = [
  { id: "articles", value: "+۱۲۰۰", label: "مقاله آموزشی", tone: "purple" as const },
  { id: "prompts", value: "+۵۰۰", label: "Prompt آماده", tone: "blue" as const },
  { id: "tools", value: "+۸۰", label: "ابزار کاربردی", tone: "cyan" as const },
  { id: "devs", value: "+۲۵K", label: "توسعه‌دهنده", tone: "purple" as const },
] as const;

const TONE: Record<(typeof HOME_PLATFORM_STATS)[number]["tone"], string> = {
  purple: "bg-[rgba(124,58,237,0.16)] text-[#C4B5FD]",
  blue: "bg-[rgba(99,102,241,0.16)] text-[#A5B4FC]",
  cyan: "bg-[rgba(6,182,212,0.14)] text-[#67E8F9]",
};

/** Horizontal stats row with small purple/blue icons. Mobile: 2-col grid. */
export function HomeStatsSection() {
  return (
    <PublicSection className="home-platform-stats home-reveal" bare aria-label="آمار پلتفرم">
      <PublicContainer size="wide">
        <dl className="grid grid-cols-2 gap-3 sm:grid-cols-4 sm:gap-0 sm:divide-x sm:divide-x-reverse sm:divide-white/[0.08] sm:rounded-2xl sm:border sm:border-white/[0.08] sm:bg-[#0B1224]/70 sm:backdrop-blur-md">
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
    strokeWidth: 1.7,
  } as const;

  if (id === "articles") {
    return (
      <svg {...common} aria-hidden>
        <path d="M7 3h8l5 5v13H7z" />
        <path d="M15 3v5h5M10 13h7M10 17h5" />
      </svg>
    );
  }
  if (id === "prompts") {
    return (
      <svg {...common} aria-hidden>
        <path d="M5 6h14v9H9l-4 3V6Z" />
      </svg>
    );
  }
  if (id === "tools") {
    return (
      <svg {...common} aria-hidden>
        <path d="M14.7 6.3a4 4 0 0 0-5.4 5.4L3 18l3 3 6.3-6.3a4 4 0 0 0 5.4-5.4l-3 3-2-2 3-3Z" />
      </svg>
    );
  }
  return (
    <svg {...common} aria-hidden>
      <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" />
      <circle cx="9" cy="7" r="3" />
      <path d="M22 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75" />
    </svg>
  );
}
