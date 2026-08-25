import Link from "next/link";
import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { PublicSection } from "@/components/ui/public/v2/public-section";
import { HomeHeroWorkspace } from "@/components/public/home/home-hero-workspace";

/**
 * Homepage hero — ~600px, equal RTL columns, Linear/Vercel density.
 * Mobile: illustration on top, copy below.
 */
export function HomeHero() {
  return (
    <PublicSection
      className="home-hero overflow-hidden pb-6 pt-8 sm:pb-8 sm:pt-10 lg:pb-10 lg:pt-12"
      bare
      aria-labelledby="home-hero-title"
    >
      <div className="pointer-events-none absolute inset-0" aria-hidden>
        <div className="absolute -top-24 left-1/2 h-[420px] w-[720px] -translate-x-1/2 rounded-full bg-[radial-gradient(circle,rgba(124,58,237,0.3),transparent_70%)] blur-3xl" />
        <div className="absolute -end-16 top-20 h-64 w-64 rounded-full bg-[radial-gradient(circle,rgba(6,182,212,0.2),transparent_70%)] blur-3xl" />
        <div className="absolute bottom-8 start-0 h-48 w-48 rounded-full bg-[radial-gradient(circle,rgba(99,102,241,0.14),transparent_70%)] blur-3xl" />
      </div>

      <PublicContainer
        size="wide"
        className="relative grid min-h-[560px] min-w-0 items-center gap-10 sm:min-h-[600px] sm:gap-12 lg:grid-cols-2 lg:gap-16"
      >
        {/* Desktop RTL: first column = right (copy). Mobile: below illustration */}
        <div className="home-hero-copy order-2 space-y-6 text-center lg:order-1 lg:text-start">
          <h1
            id="home-hero-title"
            className="text-balance text-[2rem] font-extrabold leading-[1.25] tracking-tight sm:text-[2.5rem] lg:text-[56px] lg:leading-[1.2]"
          >
            <span className="block text-white">سیستم عامل رشد</span>
            <span className="mt-1 block bg-gradient-to-l from-[#7C3AED] to-[#06B6D4] bg-clip-text text-transparent">
              توسعه‌دهندگان در عصر AI
            </span>
          </h1>

          <div className="mx-auto max-w-xl space-y-2 text-[14px] leading-8 text-[#94A3B8] sm:text-[15px] lg:mx-0">
            <p className="font-medium text-[#CBD5E1]">یاد بگیر، ابزار بساز و سریع‌تر توسعه بده.</p>
            <p>
              HelpDev مجموعه‌ای از آموزش‌ها، ابزارها، Prompt ها و اخبار دنیای توسعه است.
            </p>
          </div>

          <div className="flex flex-wrap items-center justify-center gap-3 lg:justify-start">
            <Link
              href="/learning"
              className="focus-ring inline-flex h-11 items-center justify-center rounded-xl bg-[#7C3AED] px-6 text-[14px] font-bold text-white no-underline shadow-[0_0_28px_rgba(124,58,237,0.4)] transition hover:bg-[#6D28D9] hover:shadow-[0_0_36px_rgba(124,58,237,0.5)]"
            >
              شروع مسیر
            </Link>
            <Link
              href="/articles"
              className="focus-ring inline-flex h-11 items-center justify-center rounded-xl border border-white/[0.15] bg-transparent px-6 text-[14px] font-bold text-white no-underline transition hover:border-[rgba(124,58,237,0.45)] hover:bg-white/[0.04]"
            >
              کاوش HelpDev
            </Link>
          </div>
        </div>

        {/* Desktop RTL: second column = left (illustration). Mobile: on top */}
        <div className="relative order-1 flex justify-center lg:order-2 lg:justify-start">
          <HomeHeroWorkspace />
        </div>
      </PublicContainer>
    </PublicSection>
  );
}
