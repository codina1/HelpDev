import Link from "next/link";
import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { PublicSection } from "@/components/ui/public/v2/public-section";

export const HOME_TOPIC_CHIPS = [
  "MCP",
  "Claude Code",
  "Cursor",
  ".NET",
  "React",
  "Python",
  "DevOps",
  "AI Agent",
] as const;

/** Large search surface + topic chips under the hero. */
export function HomeSearchSection() {
  return (
    <PublicSection className="home-search home-reveal pt-0 sm:pt-1" bare aria-label="جستجوی دانش">
      <PublicContainer size="wide" className="space-y-4 sm:space-y-5">
        <form
          role="search"
          aria-label="جستجوی HelpDev"
          action="/search"
          method="get"
          className="mx-auto w-full max-w-3xl"
        >
          <label className="sr-only" htmlFor="home-platform-search">
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
              id="home-platform-search"
              name="q"
              type="search"
              placeholder="هر چیزی که می‌خواهی جستجو کن..."
              className="focus-ring h-14 w-full rounded-2xl border border-white/[0.08] bg-[#0B1224] pe-24 ps-12 text-[14px] text-white outline-none placeholder:text-[#64748B] focus:border-[rgba(124,58,237,0.55)] focus:shadow-[0_0_0_3px_rgba(124,58,237,0.18),0_0_32px_rgba(124,58,237,0.2)] sm:h-16 sm:pe-28 sm:text-[15px]"
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
          {HOME_TOPIC_CHIPS.map((topic) => (
            <li key={topic}>
              <Link
                href={`/search?q=${encodeURIComponent(topic)}`}
                className="focus-ring inline-flex rounded-full border border-white/[0.08] bg-[#0B1224] px-3.5 py-1.5 text-[12px] font-semibold text-[#94A3B8] no-underline transition hover:border-[rgba(124,58,237,0.45)] hover:bg-[rgba(124,58,237,0.12)] hover:text-white hover:shadow-[0_0_18px_rgba(124,58,237,0.25)]"
              >
                {topic}
              </Link>
            </li>
          ))}
        </ul>
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
