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

/** Search ~70% width + dark-glass topic chips. */
export function HomeSearchSection() {
  return (
    <PublicSection className="home-search home-reveal pt-0 sm:pt-1" bare aria-label="جستجوی دانش">
      <PublicContainer size="wide" className="space-y-4 sm:space-y-5">
        <form
          role="search"
          aria-label="جستجوی HelpDev"
          action="/search"
          method="get"
          className="mx-auto w-full max-w-none lg:w-[70%]"
        >
          <label className="sr-only" htmlFor="home-platform-search">
            جستجو
          </label>
          <div className="relative">
            {/* Icon on the right in RTL */}
            <span
              className="pointer-events-none absolute inset-y-0 start-4 flex items-center text-[#94A3B8]"
              aria-hidden
            >
              <SearchIcon />
            </span>
            <input
              id="home-platform-search"
              name="q"
              type="search"
              placeholder="هر چیزی که می‌خواهی جستجو کن..."
              className="focus-ring h-14 w-full rounded-2xl border border-white/10 bg-[#0B1224] pe-4 ps-12 text-[14px] text-white outline-none placeholder:text-[#64748B] focus:border-[rgba(124,58,237,0.5)] focus:shadow-[0_0_0_3px_rgba(124,58,237,0.16),0_0_28px_rgba(124,58,237,0.18)]"
            />
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
                className="focus-ring inline-flex rounded-full border border-white/[0.08] bg-[#0B1224]/80 px-3.5 py-1.5 text-[12px] font-semibold text-[#94A3B8] no-underline backdrop-blur transition hover:border-[rgba(124,58,237,0.45)] hover:bg-[rgba(124,58,237,0.12)] hover:text-white hover:shadow-[0_0_18px_rgba(124,58,237,0.28)]"
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
