import Link from "next/link";
import { HomeHeroWorkspace } from "@/components/public/home/home-hero-workspace";

const TOPIC_CHIPS = [
  "MCP",
  "Claude Code",
  "Cursor",
  ".NET",
  "React",
  "Python",
  "DevOps",
  "AI Agent",
] as const;

const STATS = [
  { id: "articles", value: "+1200", label: "مقاله آموزشی", tone: "blue" as const },
  { id: "prompts", value: "+500", label: "Prompt آماده", tone: "purple" as const },
  { id: "tools", value: "+80", label: "ابزار کاربردی", tone: "blue" as const },
  { id: "roadmaps", value: "+60", label: "نقشه راه", tone: "cyan" as const },
  { id: "devs", value: "+25K", label: "توسعه‌دهنده", tone: "blue" as const },
] as const;

const STAT_ICON: Record<(typeof STATS)[number]["tone"], string> = {
  purple: "text-[#A78BFA]",
  blue: "text-[#60A5FA]",
  cyan: "text-[#67E8F9]",
};

/**
 * Homepage Hero — Premium AI landing (Linear / Vercel / Raycast).
 * Desktop: 560px · max 1280px · 50/50 RTL grid. Not full-viewport.
 */
export function HeroSection() {
  return (
    <section
      className="home-hero relative bg-[#050816]"
      aria-labelledby="home-hero-title"
    >
      <div className="pointer-events-none absolute inset-0" aria-hidden>
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_70%_45%_at_80%_0%,rgba(124,58,237,0.2),transparent_55%)]" />
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_50%_40%_at_10%_80%,rgba(37,99,235,0.14),transparent_50%)]" />
      </div>

      <div className="relative mx-auto grid w-full max-w-[1280px] grid-cols-1 items-center gap-8 px-6 py-8 sm:gap-10 sm:px-5 lg:grid-cols-2 lg:gap-6 lg:px-6 lg:py-12 xl:gap-10 min-[1440px]:min-h-[560px] min-[1440px]:gap-8 min-[1440px]:px-8 min-[1440px]:py-10">
        {/* Content — RTL right */}
        <div className="home-hero-copy order-2 flex min-w-0 w-full flex-col items-center text-center lg:order-1 lg:max-w-none lg:items-start lg:justify-center lg:pe-2 lg:text-start">
          <h1
            id="home-hero-title"
            className="w-full max-w-[36rem] whitespace-normal text-[32px] font-extrabold leading-[1.2] tracking-tight sm:text-[42px] lg:text-[44px] min-[1440px]:text-[52px]"
          >
            <span className="block text-white">سیستم عامل رشد</span>
            <span className="mt-0.5 block bg-gradient-to-l from-[#7C3AED] to-[#2563EB] bg-clip-text text-transparent">
              توسعه‌دهندگان در عصر AI
            </span>
          </h1>

          <div className="mt-3 w-full max-w-[28rem] space-y-1 text-[13px] leading-7 text-[#94A3B8] sm:text-[14px] lg:max-w-[26rem]">
            <p className="font-medium text-[#CBD5E1]">یاد بگیر، ابزار بساز و سریع‌تر توسعه بده.</p>
            <p>HelpDev مجموعه‌ای از آموزش‌ها، ابزارها، Prompt ها و اخبار دنیای توسعه است.</p>
          </div>

          <div className="mt-5 flex w-full max-w-[28rem] flex-col gap-2.5 sm:flex-row sm:justify-center lg:justify-start">
            <Link
              href="/learning"
              className="focus-ring inline-flex h-11 items-center justify-center rounded-[14px] bg-gradient-to-l from-[#7C3AED] to-[#6D28D9] px-5 text-[13px] font-bold text-white no-underline shadow-[0_0_24px_rgba(124,58,237,0.38)] transition hover:brightness-110"
            >
              شروع مسیر
            </Link>
            <Link
              href="/articles"
              className="focus-ring inline-flex h-11 items-center justify-center rounded-[14px] border border-white/[0.15] bg-transparent px-5 text-[13px] font-bold text-white no-underline transition hover:border-[rgba(124,58,237,0.45)] hover:bg-white/[0.04]"
            >
              کاوش HelpDev
            </Link>
          </div>

          <form
            role="search"
            aria-label="جستجوی HelpDev"
            action="/search"
            method="get"
            className="mt-4 w-full max-w-[28rem]"
          >
            <label className="sr-only" htmlFor="home-hero-search">
              جستجو
            </label>
            <div className="relative">
              <span
                className="pointer-events-none absolute inset-y-0 start-3.5 flex items-center text-[#94A3B8]"
                aria-hidden
              >
                <SearchIcon />
              </span>
              <input
                id="home-hero-search"
                name="q"
                type="search"
                placeholder="هر چیزی که می‌خواهی جستجو کن..."
                className="focus-ring h-[50px] w-full rounded-2xl border border-white/[0.06] bg-[#0B1224] pe-4 ps-10 text-[13px] text-white outline-none placeholder:text-[#64748B] focus:border-[rgba(124,58,237,0.45)] focus:shadow-[0_0_0_2px_rgba(124,58,237,0.12)]"
              />
            </div>
          </form>

          <ul
            className="mt-3 flex w-full max-w-[28rem] flex-wrap items-center justify-center gap-1.5 lg:justify-start"
            aria-label="موضوعات پرطرفدار"
          >
            {TOPIC_CHIPS.map((topic) => (
              <li key={topic}>
                <Link
                  href={`/search?q=${encodeURIComponent(topic)}`}
                  className="focus-ring inline-flex rounded-full border border-white/[0.08] bg-white/[0.04] px-2.5 py-1 text-[11px] font-semibold text-[#94A3B8] no-underline backdrop-blur transition hover:border-[rgba(124,58,237,0.45)] hover:bg-[rgba(124,58,237,0.12)] hover:text-white"
                >
                  {topic}
                </Link>
              </li>
            ))}
          </ul>

          <dl
            className="mt-6 grid w-full max-w-[40rem] grid-cols-2 gap-x-5 gap-y-4 sm:grid-cols-3 lg:grid-cols-5 lg:justify-items-start"
            aria-label="آمار پلتفرم"
          >
            {STATS.map((stat) => (
              <div key={stat.id} className="flex min-w-0 items-center gap-2.5">
                <span className={`shrink-0 ${STAT_ICON[stat.tone]}`} aria-hidden>
                  <StatIcon id={stat.id} />
                </span>
                <div className="text-start">
                  <dt className="sr-only">{stat.label}</dt>
                  <dd className="text-xl font-extrabold leading-none tracking-tight text-white sm:text-2xl">
                    {stat.value}
                  </dd>
                  <p className="mt-1.5 text-[11px] font-medium leading-none text-[#94A3B8]">{stat.label}</p>
                </div>
              </div>
            ))}
          </dl>
        </div>

        {/* Illustration — fills left column */}
        <div className="relative order-1 flex w-full min-w-0 items-center justify-center lg:order-2">
          <HomeHeroWorkspace />
        </div>
      </div>
    </section>
  );
}

function SearchIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" aria-hidden>
      <circle cx="11" cy="11" r="7" />
      <path d="m20 20-3-3" />
    </svg>
  );
}

function StatIcon({ id }: { id: string }) {
  const common = {
    width: 22,
    height: 22,
    viewBox: "0 0 24 24",
    fill: "none",
    stroke: "currentColor",
    strokeWidth: 1.6,
    strokeLinecap: "round" as const,
    strokeLinejoin: "round" as const,
  };

  // Grid / spreadsheet — مقالات
  if (id === "articles") {
    return (
      <svg {...common} aria-hidden>
        <rect x="3" y="3" width="18" height="18" rx="2.5" />
        <path d="M3 9.5h18M3 15h18M9.5 3v18M15 3v18" />
      </svg>
    );
  }
  // Badge / ID card — Prompt
  if (id === "prompts") {
    return (
      <svg {...common} aria-hidden>
        <path d="M7 4h10a2 2 0 0 1 2 2v14l-7-3.2L5 20V6a2 2 0 0 1 2-2Z" />
        <circle cx="12" cy="10" r="2.2" />
        <path d="M8.8 15.2c.9-1.1 2-1.7 3.2-1.7s2.3.6 3.2 1.7" />
      </svg>
    );
  }
  // Crossed wrench + screwdriver — ابزار
  if (id === "tools") {
    return (
      <svg {...common} aria-hidden>
        <path d="M14.5 4.5a3.5 3.5 0 0 0-4.7 4.7L4 15l2.8 2.8 5.8-5.8a3.5 3.5 0 0 0 4.7-4.7L14.8 9l-1.6-1.6 1.3-2.9Z" />
        <path d="M16.2 14.2 20 18l-2.2 2.2-3.8-3.8" />
        <path d="M18.5 15.5l1.8-1.8" />
      </svg>
    );
  }
  // Network nodes — نقشه راه
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
  // Single user — توسعه‌دهنده
  return (
    <svg {...common} aria-hidden>
      <circle cx="12" cy="8" r="3.2" />
      <path d="M5.5 19.5c1.4-3.2 3.6-4.8 6.5-4.8s5.1 1.6 6.5 4.8" />
    </svg>
  );
}
