import Link from "next/link";
import { Button } from "@/components/ui/ds/button";
import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { PublicSection } from "@/components/ui/public/v2/public-section";
import { HomeHeroWorkspace } from "@/components/public/home/home-hero-workspace";

/**
 * Homepage hero — equal two-column RTL layout (copy + workspace).
 * Search / chips / stats live in dedicated sections below.
 */
export function HomeHero() {
  return (
    <PublicSection
      className="home-hero overflow-hidden py-8 sm:py-10 lg:py-12"
      bare
      aria-labelledby="home-hero-title"
    >
      <div className="pointer-events-none absolute inset-0" aria-hidden>
        <div className="absolute -top-28 left-1/2 h-[460px] w-[780px] -translate-x-1/2 rounded-full bg-[radial-gradient(circle,rgba(124,58,237,0.28),transparent_70%)] blur-3xl" />
        <div className="absolute -end-20 top-16 h-72 w-72 rounded-full bg-[radial-gradient(circle,rgba(6,182,212,0.18),transparent_70%)] blur-3xl" />
        <div className="absolute bottom-10 start-0 h-56 w-56 rounded-full bg-[radial-gradient(circle,rgba(99,102,241,0.16),transparent_70%)] blur-3xl" />
      </div>

      <PublicContainer
        size="wide"
        className="relative grid min-h-[650px] min-w-0 items-center gap-10 sm:gap-12 lg:grid-cols-2 lg:gap-14"
      >
        {/* Right in RTL — copy */}
        <div className="home-hero-copy order-1 space-y-6 text-center lg:text-start">
          <h1
            id="home-hero-title"
            className="text-balance text-[1.85rem] font-extrabold leading-[1.3] tracking-tight text-white sm:text-4xl lg:text-[2.75rem] lg:leading-[1.25]"
          >
            <span className="block text-white">سیستم عامل رشد</span>
            <span className="mt-1 block bg-gradient-to-l from-[#A78BFA] via-[#7C3AED] to-[#06B6D4] bg-clip-text text-transparent">
              توسعه‌دهندگان در عصر AI
            </span>
          </h1>

          <p className="mx-auto max-w-xl text-[14px] leading-8 text-[#94A3B8] sm:text-[15px] lg:mx-0">
            یاد بگیر، ابزار بساز و سریع‌تر توسعه بده.
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

        {/* Left in RTL — illustration */}
        <div className="relative order-2 flex justify-center lg:justify-start">
          <HomeHeroWorkspace />
        </div>
      </PublicContainer>
    </PublicSection>
  );
}
