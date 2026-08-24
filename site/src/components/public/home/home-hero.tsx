import Link from "next/link";
import { Button } from "@/components/ui/ds/button";
import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { PublicSection } from "@/components/ui/public/v2/public-section";
import { HomeHeroWorkspace } from "@/components/public/home/home-hero-workspace";

const TOPIC_CHIPS = [
  "MCP",
  "Cursor",
  "Claude Code",
  ".NET",
  "React",
  "Python",
  "DevOps",
  "AI Agent",
] as const;

const HERO_STATS = [
  { id: "articles", value: "+۱۲۰۰", label: "مقاله آموزشی" },
  { id: "prompts", value: "+۵۰۰", label: "Prompt آماده" },
  { id: "tools", value: "+۸۰", label: "ابزار کاربردی" },
  { id: "devs", value: "+۲۵K", label: "توسعه‌دهنده" },
] as const;

/**
 * Homepage hero — RTL copy + developer workspace visual,
 * search, topic chips, and marketing stats.
 */
export function HomeHero() {
  return (
    <PublicSection
      className="home-hero overflow-hidden pb-8 pt-6 sm:pb-10 sm:pt-8 lg:pb-12 lg:pt-10"
      bare
      aria-labelledby="home-hero-title"
    >
      <div className="pointer-events-none absolute inset-0" aria-hidden>
        <div className="absolute -top-28 left-1/2 h-[460px] w-[780px] -translate-x-1/2 rounded-full bg-[radial-gradient(circle,rgba(124,58,237,0.28),transparent_70%)] blur-3xl" />
        <div className="absolute -end-20 top-16 h-72 w-72 rounded-full bg-[radial-gradient(circle,rgba(6,182,212,0.18),transparent_70%)] blur-3xl" />
        <div className="absolute bottom-24 start-0 h-56 w-56 rounded-full bg-[radial-gradient(circle,rgba(99,102,241,0.16),transparent_70%)] blur-3xl" />
      </div>

      <PublicContainer
        size="wide"
        className="relative grid min-w-0 items-center gap-10 sm:gap-12 lg:grid-cols-2 lg:gap-14"
      >
        {/* Text column — start/right in RTL */}
        <div className="home-hero-copy space-y-6 text-center lg:text-start">
          <h1
            id="home-hero-title"
            className="text-balance text-[1.85rem] font-extrabold leading-[1.3] tracking-tight text-white sm:text-4xl lg:text-[2.75rem] lg:leading-[1.25]"
          >
            <span className="block bg-gradient-to-l from-white via-white to-[#C4B5FD] bg-clip-text text-transparent">
              سیستم عامل رشد
            </span>
            <span className="mt-1 block bg-gradient-to-l from-[#A78BFA] via-[#7C3AED] to-[#06B6D4] bg-clip-text text-transparent">
              توسعه‌دهندگان در عصر AI
            </span>
          </h1>

          <p className="mx-auto max-w-xl text-[14px] leading-8 text-[#94A3B8] sm:text-[15px] lg:mx-0">
            یاد بگیر، ابزار بساز و سریع‌تر توسعه بده.
            <br className="hidden sm:block" />
            HelpDev مجموعه‌ای از آموزش‌ها، ابزارها، Prompt ها و اخبار دنیای توسعه است.
          </p>

          <div className="flex flex-wrap items-center justify-center gap-2.5 sm:gap-3 lg:justify-start">
            <Button
              href="/learning"
              size="lg"
              className="!bg-[#7C3AED] !shadow-[0_0_28px_rgba(124,58,237,0.4)] hover:!bg-[#6D28D9]"
            >
              شروع مسیر
            </Button>
            <Button href="/articles" variant="secondary" size="lg">
              کاوش HelpDev
            </Button>
          </div>
        </div>

        {/* Illustration — end/left in RTL */}
        <div className="relative flex justify-center lg:justify-end">
          <HomeHeroWorkspace />
        </div>
      </PublicContainer>

      {/* Search + chips + stats */}
      <PublicContainer size="wide" className="relative mt-10 space-y-5 sm:mt-12 sm:space-y-6">
        <form
          role="search"
          aria-label="جستجوی HelpDev"
          action="/search"
          method="get"
          className="mx-auto w-full max-w-3xl"
        >
          <label className="sr-only" htmlFor="home-hero-search">
            جستجو
          </label>
          <div className="relative">
            <span
              className="pointer-events-none absolute inset-y-0 start-4 flex items-center text-[#06B6D4]"
              aria-hidden
            >
              <SearchIcon />
            </span>
            <input
              id="home-hero-search"
              name="q"
              type="search"
              placeholder="هر چیزی که می‌خواهی جستجو کن..."
              className="focus-ring h-14 w-full rounded-2xl border border-white/[0.08] bg-[#0B1224]/90 pe-24 ps-12 text-[14px] text-white outline-none backdrop-blur-md placeholder:text-[#64748B] focus:border-[rgba(124,58,237,0.55)] focus:shadow-[0_0_0_3px_rgba(124,58,237,0.18),0_0_32px_rgba(124,58,237,0.2)] sm:h-16 sm:pe-28 sm:text-[15px]"
            />
            <button
              type="submit"
              className="focus-ring absolute inset-y-2 end-2 inline-flex items-center rounded-xl bg-[#7C3AED] px-3.5 text-[12px] font-bold text-white shadow-[0_0_20px_rgba(124,58,237,0.35)] transition hover:bg-[#6D28D9] sm:px-4 sm:text-[13px]"
            >
              جستجو
            </button>
          </div>
        </form>

        <ul
          className="flex flex-wrap items-center justify-center gap-2"
          aria-label="موضوعات پرطرفدار"
        >
          {TOPIC_CHIPS.map((topic) => (
            <li key={topic}>
              <Link
                href={`/search?q=${encodeURIComponent(topic)}`}
                className="focus-ring inline-flex rounded-full border border-white/[0.08] bg-[#0B1224]/70 px-3.5 py-1.5 text-[12px] font-semibold text-[#94A3B8] no-underline backdrop-blur transition hover:border-[rgba(124,58,237,0.45)] hover:bg-[rgba(124,58,237,0.12)] hover:text-white hover:shadow-[0_0_18px_rgba(124,58,237,0.25)]"
              >
                {topic}
              </Link>
            </li>
          ))}
        </ul>

        <dl
          className="grid grid-cols-2 gap-3 sm:grid-cols-4 sm:gap-4"
          aria-label="آمار پلتفرم"
        >
          {HERO_STATS.map((stat) => (
            <div
              key={stat.id}
              className="rounded-2xl border border-white/[0.08] bg-[#0B1224]/75 px-3 py-3.5 text-center backdrop-blur-md sm:px-4 sm:py-4"
            >
              <dt className="sr-only">{stat.label}</dt>
              <dd className="text-[1.15rem] font-extrabold tracking-tight text-white sm:text-xl">
                {stat.value}
              </dd>
              <p className="mt-1 text-[11px] font-semibold text-[#94A3B8] sm:text-[12px]">
                {stat.label}
              </p>
            </div>
          ))}
        </dl>
      </PublicContainer>
    </PublicSection>
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
