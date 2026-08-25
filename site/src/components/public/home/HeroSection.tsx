import Link from "next/link";
import { PublicContainer } from "@/components/ui/public/v2/public-container";
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
  { id: "tools", value: "+80", label: "ابزار کاربردی", tone: "cyan" as const },
  { id: "devs", value: "+25K", label: "توسعه‌دهنده", tone: "purple" as const },
] as const;

const STAT_TONE: Record<(typeof STATS)[number]["tone"], string> = {
  purple: "bg-[rgba(124,58,237,0.16)] text-[#C4B5FD]",
  blue: "bg-[rgba(37,99,235,0.16)] text-[#93C5FD]",
  cyan: "bg-[rgba(6,182,212,0.14)] text-[#67E8F9]",
};

/**
 * Homepage Hero — AI Developer Platform (Linear / Vercel / Raycast).
 * RTL: content on the right, workspace illustration on the left.
 */
export function HeroSection() {
  return (
    <section
      className="home-hero relative overflow-hidden bg-[#050816]"
      aria-labelledby="home-hero-title"
    >
      <div className="pointer-events-none absolute inset-0" aria-hidden>
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_80%_50%_at_70%_0%,rgba(124,58,237,0.18),transparent_55%)]" />
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_60%_40%_at_10%_80%,rgba(37,99,235,0.12),transparent_50%)]" />
        <div className="absolute -top-28 left-1/2 h-[440px] w-[760px] -translate-x-1/2 rounded-full bg-[radial-gradient(circle,rgba(124,58,237,0.28),transparent_68%)] blur-3xl" />
        <div className="absolute -end-20 top-24 h-72 w-72 rounded-full bg-[radial-gradient(circle,rgba(37,99,235,0.22),transparent_70%)] blur-3xl" />
        <div className="absolute bottom-10 start-4 h-52 w-52 rounded-full bg-[radial-gradient(circle,rgba(6,182,212,0.14),transparent_70%)] blur-3xl" />
      </div>

      <PublicContainer
        size="wide"
        className="relative flex min-h-[560px] flex-col justify-center gap-10 py-10 sm:min-h-[600px] sm:gap-12 sm:py-12 lg:min-h-[650px] lg:gap-14 lg:py-0"
      >
        <div className="grid min-w-0 items-center gap-10 lg:grid-cols-2 lg:gap-16">
          {/* Desktop RTL: first column = right (content). Mobile: below illustration */}
          <div className="home-hero-copy order-2 space-y-6 text-center lg:order-1 lg:text-start">
            <h1
              id="home-hero-title"
              className="text-balance text-[36px] font-extrabold leading-[1.3] tracking-tight sm:text-[44px] lg:text-[56px]"
            >
              <span className="block text-white">سیستم عامل رشد</span>
              <span className="mt-1 block bg-gradient-to-l from-[#7C3AED] to-[#2563EB] bg-clip-text text-transparent">
                توسعه‌دهندگان در عصر AI
              </span>
            </h1>

            <div className="mx-auto w-full max-w-[400px] space-y-2 text-[14px] leading-8 text-[#94A3B8] sm:text-[15px] lg:mx-0">
              <p className="font-medium text-[#CBD5E1]">یاد بگیر، ابزار بساز و سریع‌تر توسعه بده.</p>
              <p>HelpDev مجموعه‌ای از آموزش‌ها، ابزارها، Prompt ها و اخبار دنیای توسعه است.</p>
            </div>

            <div className="flex flex-col items-stretch gap-3 sm:flex-row sm:flex-wrap sm:items-center sm:justify-center lg:justify-start">
              <Link
                href="/learning"
                className="focus-ring inline-flex h-12 items-center justify-center rounded-[14px] bg-gradient-to-l from-[#7C3AED] to-[#6D28D9] px-6 text-[14px] font-bold text-white no-underline shadow-[0_0_28px_rgba(124,58,237,0.4)] transition hover:brightness-110 hover:shadow-[0_0_36px_rgba(124,58,237,0.5)] sm:w-auto"
              >
                شروع مسیر
              </Link>
              <Link
                href="/articles"
                className="focus-ring inline-flex h-12 items-center justify-center rounded-[14px] border border-white/[0.15] bg-transparent px-6 text-[14px] font-bold text-white no-underline transition hover:border-[rgba(124,58,237,0.45)] hover:bg-white/[0.04] sm:w-auto"
              >
                کاوش HelpDev
              </Link>
            </div>

            <form
              role="search"
              aria-label="جستجوی HelpDev"
              action="/search"
              method="get"
              className="mx-auto w-full max-w-none lg:mx-0 lg:w-[420px]"
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
                  className="focus-ring h-14 w-full rounded-2xl border border-white/10 bg-[#0B1224] pe-4 ps-12 text-[14px] text-white outline-none placeholder:text-[#64748B] focus:border-[rgba(124,58,237,0.5)] focus:shadow-[0_0_0_3px_rgba(124,58,237,0.16),0_0_28px_rgba(124,58,237,0.18)]"
                />
              </div>
            </form>

            <ul
              className="flex flex-wrap items-center justify-center gap-2 lg:justify-start"
              aria-label="موضوعات پرطرفدار"
            >
              {TOPIC_CHIPS.map((topic) => (
                <li key={topic}>
                  <Link
                    href={`/search?q=${encodeURIComponent(topic)}`}
                    className="focus-ring inline-flex rounded-full border border-white/10 bg-white/[0.04] px-3.5 py-1.5 text-[12px] font-semibold text-[#94A3B8] no-underline backdrop-blur transition hover:border-[rgba(124,58,237,0.45)] hover:bg-[rgba(124,58,237,0.12)] hover:text-white hover:shadow-[0_0_18px_rgba(124,58,237,0.28)]"
                  >
                    {topic}
                  </Link>
                </li>
              ))}
            </ul>
          </div>

          {/* Desktop RTL: second column = left (illustration). Mobile: on top */}
          <div className="relative order-1 flex justify-center lg:order-2 lg:justify-start">
            <HomeHeroWorkspace />
          </div>
        </div>

        <dl
          className="grid grid-cols-2 gap-3 sm:grid-cols-4 sm:gap-0 sm:divide-x sm:divide-x-reverse sm:divide-white/[0.08] sm:rounded-2xl sm:border sm:border-white/[0.08] sm:bg-[#0B1224]/70 sm:backdrop-blur-md"
          aria-label="آمار پلتفرم"
        >
          {STATS.map((stat) => (
            <div
              key={stat.id}
              className="flex items-center gap-3 rounded-2xl border border-white/[0.08] bg-[#0B1224] px-3 py-3.5 sm:justify-center sm:rounded-none sm:border-0 sm:bg-transparent sm:px-4 sm:py-5"
            >
              <span
                className={`flex h-9 w-9 shrink-0 items-center justify-center rounded-xl ${STAT_TONE[stat.tone]}`}
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
