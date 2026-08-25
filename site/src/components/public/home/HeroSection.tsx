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
  { id: "prompts", value: "+500", label: "Prompt آماده", tone: "blue" as const },
  { id: "tools", value: "+80", label: "ابزار کاربردی", tone: "purple" as const },
  { id: "devs", value: "+25K", label: "توسعه‌دهنده", tone: "blue" as const },
] as const;

const STAT_ICON: Record<(typeof STATS)[number]["tone"], string> = {
  purple: "text-[#A78BFA]",
  blue: "text-[#60A5FA]",
};

/**
 * Homepage Hero — Premium AI landing (Linear / Vercel / Raycast).
 * Desktop: 560px · max 1280px · 50/50 RTL grid. Not full-viewport.
 */
export function HeroSection() {
  return (
    <section
      className="home-hero relative overflow-hidden bg-[#050816]"
      aria-labelledby="home-hero-title"
    >
      <div className="pointer-events-none absolute inset-0" aria-hidden>
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_70%_45%_at_80%_0%,rgba(124,58,237,0.2),transparent_55%)]" />
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_50%_40%_at_10%_80%,rgba(37,99,235,0.14),transparent_50%)]" />
      </div>

      <div className="relative mx-auto grid w-full max-w-[1280px] grid-cols-1 items-center gap-4 px-4 py-5 sm:px-6 lg:h-[560px] lg:max-h-[560px] lg:grid-cols-2 lg:gap-5 lg:overflow-hidden lg:px-8 lg:py-0">
        {/* Content — RTL right, inset from edge */}
        <div className="home-hero-copy order-2 flex min-w-0 flex-col items-center text-center lg:order-1 lg:items-start lg:justify-center lg:pr-[56px] lg:text-start">
          <h1
            id="home-hero-title"
            className="max-w-[520px] whitespace-normal text-[34px] font-extrabold leading-[1.2] tracking-tight sm:text-[42px] lg:text-[52px]"
          >
            <span className="block text-white">سیستم عامل رشد</span>
            <span className="mt-0.5 block bg-gradient-to-l from-[#7C3AED] to-[#2563EB] bg-clip-text text-transparent">
              توسعه‌دهندگان در عصر AI
            </span>
          </h1>

          <div className="mt-2.5 w-full max-w-[420px] space-y-1 text-[13px] leading-7 text-[#94A3B8] sm:text-[14px]">
            <p className="font-medium text-[#CBD5E1]">یاد بگیر، ابزار بساز و سریع‌تر توسعه بده.</p>
            <p>HelpDev مجموعه‌ای از آموزش‌ها، ابزارها، Prompt ها و اخبار دنیای توسعه است.</p>
          </div>

          <div className="mt-4 flex w-full max-w-[400px] flex-col gap-2.5 sm:flex-row sm:justify-center lg:justify-start">
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
            className="mt-3 w-full max-w-[400px] lg:w-[400px]"
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
            className="mt-2 flex max-w-[400px] flex-wrap items-center justify-center gap-1.5 lg:justify-start"
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
            className="mt-5 flex w-full max-w-[520px] flex-wrap items-center justify-center gap-x-9 gap-y-3 lg:justify-start"
            aria-label="آمار پلتفرم"
          >
            {STATS.map((stat) => (
              <div key={stat.id} className="flex min-w-0 items-center gap-2.5">
                <span className={`shrink-0 ${STAT_ICON[stat.tone]}`} aria-hidden>
                  <StatIcon id={stat.id} />
                </span>
                <div className="text-start">
                  <dt className="sr-only">{stat.label}</dt>
                  <dd className="text-2xl font-extrabold leading-none tracking-tight text-white">
                    {stat.value}
                  </dd>
                  <p className="mt-1.5 text-[11px] font-medium leading-none text-[#94A3B8]">{stat.label}</p>
                </div>
              </div>
            ))}
          </dl>
        </div>

        {/* Illustration — RTL left, vertically centered */}
        <div className="relative order-1 flex items-center justify-center lg:order-2 lg:h-[500px] lg:w-[650px] lg:max-w-none lg:justify-self-center">
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
