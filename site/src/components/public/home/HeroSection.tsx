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
  { id: "articles", value: "+1200", label: "مقاله آموزشی", tone: "purple" as const },
  { id: "prompts", value: "+500", label: "Prompt آماده", tone: "cyan" as const },
  { id: "tools", value: "+80", label: "ابزار کاربردی", tone: "purple" as const },
  { id: "devs", value: "+25K", label: "توسعه‌دهنده", tone: "cyan" as const },
] as const;

const STAT_ICON: Record<(typeof STATS)[number]["tone"], string> = {
  purple: "text-[#A78BFA]",
  cyan: "text-[#22D3EE]",
};

/**
 * Homepage Hero — compact Premium AI landing (Linear / Vercel / Raycast).
 * Desktop: 600px · max 1280px · 50/50 RTL grid.
 */
export function HeroSection() {
  return (
    <section
      className="home-hero relative overflow-hidden bg-[#050816]"
      aria-labelledby="home-hero-title"
    >
      <div className="pointer-events-none absolute inset-0" aria-hidden>
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_70%_45%_at_75%_10%,rgba(124,58,237,0.16),transparent_55%)]" />
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_50%_40%_at_15%_70%,rgba(6,182,212,0.1),transparent_50%)]" />
        <div className="absolute -top-24 end-[18%] h-[360px] w-[520px] rounded-full bg-[radial-gradient(circle,rgba(124,58,237,0.26),transparent_68%)] blur-3xl" />
        <div className="absolute bottom-0 start-[8%] h-56 w-56 rounded-full bg-[radial-gradient(circle,rgba(6,182,212,0.16),transparent_70%)] blur-3xl" />
      </div>

      <div className="relative mx-auto grid w-full max-w-[1280px] grid-cols-1 items-center gap-8 px-4 py-8 sm:px-6 sm:py-10 lg:h-[600px] lg:grid-cols-2 lg:gap-10 lg:px-8 lg:py-0">
        {/* Content — RTL right column */}
        <div className="home-hero-copy order-2 flex min-w-0 flex-col items-center text-center lg:order-1 lg:items-start lg:text-start">
          <h1
            id="home-hero-title"
            className="max-w-[520px] text-balance text-[36px] font-extrabold leading-[1.25] tracking-tight sm:text-[44px] lg:text-[56px]"
          >
            <span className="block text-white">سیستم عامل رشد</span>
            <span className="mt-0.5 block bg-gradient-to-l from-[#7C3AED] to-[#2563EB] bg-clip-text text-transparent">
              توسعه‌دهندگان در عصر AI
            </span>
          </h1>

          <div className="mt-4 w-full max-w-[420px] space-y-1.5 text-[14px] leading-7 text-[#94A3B8] sm:text-[15px] sm:leading-8">
            <p className="font-medium text-[#CBD5E1]">یاد بگیر، ابزار بساز و سریع‌تر توسعه بده.</p>
            <p>HelpDev مجموعه‌ای از آموزش‌ها، ابزارها، Prompt ها و اخبار دنیای توسعه است.</p>
          </div>

          <div className="mt-8 flex w-full max-w-[420px] flex-col gap-3 sm:flex-row sm:flex-wrap sm:justify-center lg:justify-start">
            <Link
              href="/learning"
              className="focus-ring inline-flex h-12 items-center justify-center rounded-[14px] bg-gradient-to-l from-[#7C3AED] to-[#6D28D9] px-6 text-[14px] font-bold text-white no-underline shadow-[0_0_28px_rgba(124,58,237,0.4)] transition hover:brightness-110"
            >
              شروع مسیر
            </Link>
            <Link
              href="/articles"
              className="focus-ring inline-flex h-12 items-center justify-center rounded-[14px] border border-white/[0.15] bg-transparent px-6 text-[14px] font-bold text-white no-underline transition hover:border-[rgba(124,58,237,0.45)] hover:bg-white/[0.04]"
            >
              کاوش HelpDev
            </Link>
          </div>

          <form
            role="search"
            aria-label="جستجوی HelpDev"
            action="/search"
            method="get"
            className="mt-5 w-full max-w-[430px] lg:w-[430px]"
          >
            <label className="sr-only" htmlFor="home-hero-search">
              جستجو
            </label>
            <div className="relative">
              <span
                className="pointer-events-none absolute inset-y-0 start-4 flex items-center text-[#94A3B8]"
                aria-hidden
              >
                <SearchIcon />
              </span>
              <input
                id="home-hero-search"
                name="q"
                type="search"
                placeholder="هر چیزی که می‌خواهی جستجو کن..."
                className="focus-ring h-14 w-full rounded-2xl border border-white/10 bg-[#0B1224] pe-4 ps-12 text-[14px] text-white outline-none placeholder:text-[#64748B] focus:border-[rgba(124,58,237,0.5)] focus:shadow-[0_0_0_3px_rgba(124,58,237,0.16)]"
              />
            </div>
          </form>

          <ul
            className="mt-3.5 flex max-w-[430px] flex-wrap items-center justify-center gap-2 lg:justify-start"
            aria-label="موضوعات پرطرفدار"
          >
            {TOPIC_CHIPS.map((topic) => (
              <li key={topic}>
                <Link
                  href={`/search?q=${encodeURIComponent(topic)}`}
                  className="focus-ring inline-flex rounded-full border border-white/10 bg-white/[0.04] px-3 py-1.5 text-[12px] font-semibold text-[#94A3B8] no-underline backdrop-blur transition hover:border-[rgba(124,58,237,0.45)] hover:bg-[rgba(124,58,237,0.12)] hover:text-white hover:shadow-[0_0_18px_rgba(124,58,237,0.28)]"
                >
                  {topic}
                </Link>
              </li>
            ))}
          </ul>

          <dl
            className="mt-7 flex w-full max-w-[520px] flex-wrap items-start justify-center gap-x-6 gap-y-4 sm:gap-x-8 lg:justify-start"
            aria-label="آمار پلتفرم"
          >
            {STATS.map((stat) => (
              <div key={stat.id} className="flex min-w-[6.5rem] items-center gap-2.5">
                <span className={`shrink-0 ${STAT_ICON[stat.tone]}`} aria-hidden>
                  <StatIcon id={stat.id} />
                </span>
                <div className="min-w-0 text-start">
                  <dt className="sr-only">{stat.label}</dt>
                  <dd className="text-[15px] font-extrabold leading-none tracking-tight text-white sm:text-base">
                    {stat.value}
                  </dd>
                  <p className="mt-1 text-[11px] font-medium leading-none text-[#94A3B8]">
                    {stat.label}
                  </p>
                </div>
              </div>
            ))}
          </dl>
        </div>

        {/* Illustration — RTL left column */}
        <div className="relative order-1 flex justify-center lg:order-2 lg:h-[500px] lg:w-[600px] lg:justify-self-center">
          <HomeHeroWorkspace />
        </div>
      </div>
    </section>
  );
}

function SearchIcon() {
  return (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" aria-hidden>
      <circle cx="11" cy="11" r="7" />
      <path d="m20 20-3-3" />
    </svg>
  );
}

function StatIcon({ id }: { id: string }) {
  const common = {
    width: 18,
    height: 18,
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
