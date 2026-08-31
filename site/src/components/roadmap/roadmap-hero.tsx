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
      <path d="M9.9 9.4a2.2 2.2 0 0 1 4.2.8c0 1.5-2.1 1.8-2.1 3.2" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" />
      <circle cx="12" cy="16.6" r="0.95" fill="currentColor" />
    </svg>
  );
}

/** Hero: text right (RTL) / neon roadmap illustration left. */
export function RoadmapHero() {
  return (
    <section className="relative overflow-hidden bg-[#030713] pb-3 pt-5" aria-labelledby="roadmap-hero-heading">
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_50%_70%_at_22%_40%,rgba(124,58,237,0.16),transparent_70%)]"
        aria-hidden
      />
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_42%_58%_at_82%_32%,rgba(59,130,246,0.12),transparent_70%)]"
        aria-hidden
      />

      <RoadmapContainer>
        <div className="relative overflow-hidden rounded-[18px] border border-white/[0.07] bg-gradient-to-bl from-[#0C1226] via-[#080D1E] to-[#0F0A22]">
          <div
            className="pointer-events-none absolute inset-x-[10%] bottom-0 h-20 bg-[radial-gradient(ellipse_at_center,rgba(124,58,237,0.18),transparent_70%)] blur-xl"
            aria-hidden
          />
          <div
            className="relative grid min-h-[250px] items-center gap-6 px-6 py-9 sm:px-8 md:h-[290px] md:min-h-[290px] md:grid-cols-2 md:gap-10 md:px-12 md:py-0 lg:px-16"
            dir="ltr"
          >
            <div className="order-2 max-w-[600px] text-center md:order-1 md:text-left" dir="rtl">
              <p className="text-[13px] font-bold tracking-[0.06em] text-[#A78BFA] sm:text-[14px]">
                Roadmap
              </p>
              <h1
                id="roadmap-hero-heading"
                className="mt-2.5 whitespace-nowrap text-[26px] font-extrabold leading-[1.32] tracking-tight text-white sm:whitespace-normal sm:text-[32px] md:text-[36px] lg:text-[40px]"
              >
                مسیر یادگیری{" "}
                <span className="bg-gradient-to-l from-[#60A5FA] via-[#A855F7] to-[#C084FC] bg-clip-text text-transparent">
                  توسعه‌دهنده حرفه‌ای
                </span>
              </h1>
              <p className="mt-4 max-w-[560px] text-[14px] leading-8 text-[#94A3B8] sm:text-[15px]">
                نقشه راه یادگیری برنامه‌نویسی و مهارت‌های فنی را مرحله به مرحله دنبال کنید تا به یک توسعه‌دهنده حرفه‌ای تبدیل شوید.
              </p>

              <div className="mt-6 flex flex-col items-stretch gap-3 sm:flex-row sm:items-center sm:justify-center md:justify-start">
                <Link
                  href="#roadmap-paths"
                  className="focus-ring inline-flex h-12 items-center justify-center gap-2 rounded-xl bg-gradient-to-l from-[#7C3AED] to-[#6D28D9] px-6 text-[14px] font-bold text-white no-underline shadow-[0_0_18px_rgba(124,58,237,0.3)] transition hover:brightness-110"
                >
                  <GridIcon className="h-4 w-4 shrink-0" />
                  نمایش همه مسیرها
                </Link>
                <Link
                  href="#roadmap-guide"
                  className="focus-ring inline-flex h-12 items-center justify-center gap-2 rounded-xl border border-white/[0.1] bg-[#0F1626] px-6 text-[14px] font-bold text-[#E5E7EB] no-underline transition hover:border-[rgba(168,85,247,0.4)] hover:text-white"
                >
                  <HelpIcon className="h-4 w-4 shrink-0" />
                  راهنمای استفاده
                </Link>
              </div>
            </div>

            <div className="order-1 flex items-center justify-center md:order-2 md:justify-end">
              <div className="relative flex h-[180px] w-full max-w-[340px] items-center justify-center sm:h-[220px] sm:max-w-[440px] md:h-[270px] md:max-w-[560px]">
                <span
                  className="pointer-events-none absolute inset-0 rounded-full bg-[radial-gradient(circle,rgba(99,102,241,0.3),rgba(59,130,246,0.12)_52%,transparent_74%)] blur-2xl"
                  aria-hidden
                />
                <img
                  src={ROADMAP_HERO_IMAGE_SRC}
                  alt=""
                  width={560}
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
