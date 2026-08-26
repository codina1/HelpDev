import Link from "next/link";
import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { PublicSection } from "@/components/ui/public/v2/public-section";
import type { ToolSummaryDto } from "@/lib/api/toolbox";

const FALLBACK_TOOLS = [
  { id: "json", title: "JSON Formatter", slug: "json-formatter" },
  { id: "jwt", title: "JWT Decoder", slug: "jwt-decoder" },
  { id: "regex", title: "Regex Tester", slug: "regex-tester" },
  { id: "sql", title: "SQL Formatter", slug: "sql-formatter" },
  { id: "base64", title: "Base64 Encode", slug: "base64" },
  { id: "uuid", title: "UUID Generator", slug: "uuid-generator" },
  { id: "hash", title: "Hash Generator", slug: "hash-generator" },
  { id: "cron", title: "Cron Explainer", slug: "cron-explainer" },
] as const;

const PROMPT_TAGS = ["Cursor", "Claude", "ChatGPT", "Copilot"] as const;

type HomeHubSectionProps = {
  tools?: ToolSummaryDto[];
};

/**
 * Two-column hub: Developer Toolbox (start/right) + Prompt Lab (end/left).
 */
export function HomeHubSection({ tools = [] }: HomeHubSectionProps) {
  const toolItems =
    tools.length > 0
      ? tools.slice(0, 8).map((tool) => ({
          id: tool.id,
          title: tool.title,
          href: `/toolbox?tool=${encodeURIComponent(tool.slug)}`,
        }))
      : FALLBACK_TOOLS.map((tool) => ({
          id: tool.id,
          title: tool.title,
          href: `/toolbox?q=${encodeURIComponent(tool.slug)}`,
        }));

  return (
    <PublicSection
      className="home-hub home-reveal"
      bare
      aria-labelledby="home-hub-heading"
    >
      <PublicContainer size="wide">
        <h2 id="home-hub-heading" className="sr-only">
          ابزارها و Prompt Lab
        </h2>
        <div className="grid gap-4 md:grid-cols-2 lg:gap-5">
          {/* Right in RTL */}
          <section
            aria-labelledby="home-toolbox-heading"
            className="rounded-2xl border border-white/[0.08] bg-[#0B1224] p-5 sm:p-6"
          >
            <div className="mb-4 flex items-center justify-between gap-3">
              <h3 id="home-toolbox-heading" className="text-[16px] font-extrabold text-white">
                Developer Toolbox
              </h3>
              <Link
                href="/toolbox"
                className="focus-ring text-[12px] font-semibold text-[#06B6D4] no-underline hover:text-white"
              >
                همه ابزارها
              </Link>
            </div>
            <ul className="grid grid-cols-1 gap-2 sm:grid-cols-2 min-[1440px]:grid-cols-3">
              {toolItems.map((tool) => (
                <li key={tool.id}>
                  <Link
                    href={tool.href}
                    className="focus-ring flex items-center gap-2.5 rounded-xl border border-white/[0.06] bg-white/[0.02] px-3 py-2.5 text-[13px] font-semibold text-[#CBD5E1] no-underline transition hover:border-[rgba(6,182,212,0.35)] hover:bg-[rgba(6,182,212,0.08)] hover:text-white"
                  >
                    <span
                      className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-[rgba(6,182,212,0.12)] text-[#67E8F9]"
                      aria-hidden
                    >
                      <WrenchIcon />
                    </span>
                    <span className="truncate">{tool.title}</span>
                  </Link>
                </li>
              ))}
            </ul>
          </section>

          {/* Left in RTL */}
          <section
            aria-labelledby="home-prompt-heading"
            className="relative overflow-hidden rounded-2xl border border-[rgba(124,58,237,0.35)] bg-gradient-to-bl from-[rgba(124,58,237,0.22)] via-[#0B1224] to-[#0B1224] p-5 shadow-[0_0_40px_rgba(124,58,237,0.18)] sm:p-6"
          >
            <div
              className="pointer-events-none absolute -end-10 -top-10 h-40 w-40 rounded-full bg-[rgba(124,58,237,0.25)] blur-3xl"
              aria-hidden
            />
            <div className="relative">
              <p className="text-[12px] font-bold tracking-wide text-[#C4B5FD]">Prompt Lab</p>
              <h3 id="home-prompt-heading" className="mt-2 text-[1.15rem] font-extrabold text-white sm:text-xl">
                پرامپت‌های آماده برای کدنویسی با AI
              </h3>
              <p className="mt-2 text-[13px] leading-7 text-[#94A3B8]">
                مجموعه پرامپت‌های مهندسی برای Cursor، Claude و Copilot — آماده استفاده در پروژه واقعی.
              </p>
              <ul className="mt-5 flex flex-wrap gap-2" aria-label="ابزارهای پشتیبانی‌شده">
                {PROMPT_TAGS.map((tag) => (
                  <li
                    key={tag}
                    className="rounded-full border border-white/[0.1] bg-white/[0.04] px-3 py-1 text-[11px] font-semibold text-[#E2E8F0]"
                  >
                    {tag}
                  </li>
                ))}
              </ul>
              <Link
                href="/prompt-lab"
                className="focus-ring mt-6 inline-flex items-center rounded-xl bg-[#7C3AED] px-4 py-2.5 text-[13px] font-bold text-white no-underline shadow-[0_0_24px_rgba(124,58,237,0.35)] transition hover:bg-[#6D28D9]"
              >
                ورود به Prompt Lab
              </Link>
            </div>
          </section>
        </div>
      </PublicContainer>
    </PublicSection>
  );
}

function WrenchIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden>
      <path d="M14.7 6.3a4 4 0 0 0-5.4 5.4L3 18l3 3 6.3-6.3a4 4 0 0 0 5.4-5.4l-3 3-2-2 3-3Z" />
    </svg>
  );
}
