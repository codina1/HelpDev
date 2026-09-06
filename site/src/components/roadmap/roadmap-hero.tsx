import Link from "next/link";
import { RoadmapContainer } from "@/components/roadmap/roadmap-container";

export const ROADMAP_HERO_IMAGE_SRC = "/roadmap/hero-roadmap.png";

function GridIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <rect x="3.5" y="3.5" width="7" height="7" rx="2" stroke="currentColor" strokeWidth="1.8" />
      <rect x="13.5" y="3.5" width="7" height="7" rx="2" stroke="currentColor" strokeWidth="1.8" />
      <rect x="3.5" y="13.5" width="7" height="7" rx="2" stroke="currentColor" strokeWidth="1.8" />
      <rect x="13.5" y="13.5" width="7" height="7" rx="2" stroke="currentColor" strokeWidth="1.8" />
    </svg>
  );
}

function HelpIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <circle cx="12" cy="12" r="8.4" stroke="currentColor" strokeWidth="1.7" />
      <path
        d="M9.9 9.4a2.2 2.2 0 0 1 4.2.8c0 1.5-2.1 1.8-2.1 3.2"
        stroke="currentColor"
        strokeWidth="1.7"
        strokeLinecap="round"
      />
      <circle cx="12" cy="16.6" r="0.95" fill="currentColor" />
    </svg>
  );
}

/** Hero: text LEFT (RTL start) / neon roadmap illustration RIGHT · ~275px. */
export function RoadmapHero() {
  return (
    <section
      className="relative overflow-hidden bg-[#030713] pb-2 pt-3 md:pb-3 md:pt-4"
      aria-labelledby="roadmap-hero-heading"
    >
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_50%_70%_at_22%_40%,rgba(124,58,237,0.16),transparent_70%)]"
        aria-hidden
      />
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_42%_58%_at_82%_32%,rgba(59,130,246,0.12),transparent_70%)]"
        aria-hidden
      />

      <RoadmapContainer>
        <div className="relative rounded-[16px] border border-white/[0.08] bg-gradient-to-bl from-[#0C1226] via-[#080D1E] to-[#0F0A22]">
          <div
            className="pointer-events-none absolute inset-x-[10%] bottom-0 h-16 bg-[radial-gradient(ellipse_at_center,rgba(124,58,237,0.18),transparent_70%)] blur-xl"
            aria-hidden
          />
          <div
            className="relative grid items-center gap-4 px-5 py-5 sm:px-7 sm:py-6 md:h-[275px] md:grid-cols-[minmax(0,48%)_minmax(0,52%)] md:gap-6 md:px-8 md:py-5 lg:px-10"
            dir="ltr"
          >
            <div className="order-2 min-w-0 md:order-1" dir="rtl">
              <p className="text-[13px] font-bold tracking-[0.06em] text-[#A78BFA]">Roadmap</p>
              <h1
                id="roadmap-hero-heading"
                className="mt-1.5 whitespace-nowrap text-[20px] font-extrabold leading-[1.3] tracking-tight text-white sm:text-[26px] md:text-[28px] min-[900px]:text-[30px] lg:text-[34px]"
              >
                مسیر یادگیری{" "}
                <span className="bg-gradient-to-l from-[#60A5FA] via-[#A855F7] to-[#C084FC] bg-clip-text text-transparent">
                  توسعه‌دهنده
                </span>{" "}
                حرفه‌ای
              </h1>
              <p className="mt-2 max-w-[36rem] text-[13px] leading-7 text-[#94A3B8] line-clamp-2 sm:text-[14px] sm:leading-7">
                نقشه راه یادگیری برنامه‌نویسی و مهارت‌های فنی را مرحله به مرحله دنبال کنید تا به یک
                توسعه‌دهنده حرفه‌ای تبدیل شوید.
              </p>

              <div className="mt-4 flex flex-wrap items-center gap-2.5">
                <Link
                  href="#roadmap-paths"
                  className="focus-ring inline-flex h-11 items-center justify-center gap-2 rounded-xl bg-gradient-to-l from-[#7C3AED] to-[#6D28D9] px-5 text-[13px] font-bold text-white no-underline shadow-[0_0_18px_rgba(124,58,237,0.3)] transition hover:brightness-110"
                >
                  <GridIcon className="h-4 w-4 shrink-0" />
                  نمایش همه مسیرها
                </Link>
                <Link
                  href="#roadmap-guide"
                  className="focus-ring inline-flex h-11 items-center justify-center gap-2 rounded-xl border border-[rgba(168,85,247,0.45)] bg-[#0F1626] px-5 text-[13px] font-bold text-[#E5E7EB] no-underline transition hover:border-[#A78BFA] hover:text-white"
                >
                  <HelpIcon className="h-4 w-4 shrink-0 text-[#A78BFA]" />
                  راهنمای استفاده
                </Link>
              </div>
            </div>

            <div className="order-1 flex min-h-0 items-center justify-center md:order-2 md:h-full md:justify-end">
              <div className="relative flex h-[180px] w-full items-center justify-center sm:h-[210px] md:h-full md:max-h-[250px]">
                <span
                  className="pointer-events-none absolute inset-0 rounded-full bg-[radial-gradient(circle,rgba(99,102,241,0.32),rgba(59,130,246,0.12)_52%,transparent_74%)] blur-2xl"
                  aria-hidden
                />
                <img
                  src={ROADMAP_HERO_IMAGE_SRC}
                  alt=""
                  width={640}
                  height={280}
                  loading="eager"
                  fetchPriority="high"
                  decoding="async"
                  className="relative h-full w-full object-contain drop-shadow-[0_18px_46px_rgba(99,102,241,0.4)]"
                />
              </div>
            </div>
          </div>
        </div>
      </RoadmapContainer>
    </section>
  );
}
