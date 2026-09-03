import Link from "next/link";
import { PromptLabContainer } from "@/components/prompt-lab/prompt-lab-container";
import {
  PROMPT_LAB_HERO_EYEBROW,
  PROMPT_LAB_HERO_IMAGE_SRC,
  PROMPT_LAB_HERO_SUBTITLE,
  PROMPT_LAB_HERO_TITLE,
} from "@/data/prompt-lab";

function PlusIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path d="M12 5v14M5 12h14" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
    </svg>
  );
}

function FlaskIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M9 3h6M10 3v6.2L5.8 17a3.2 3.2 0 0 0 2.7 4.8h7a3.2 3.2 0 0 0 2.7-4.8L14 9.2V3"
        stroke="currentColor"
        strokeWidth="1.7"
        strokeLinejoin="round"
      />
      <path d="M8.2 14h7.6" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" />
    </svg>
  );
}

/** Reference hero: copy right (RTL) / neon flask illustration left. */
export function PromptLabHero() {
  return (
    <section
      className="relative overflow-hidden bg-[#070b18] pb-6 pt-6 md:pt-8"
      aria-labelledby="prompt-lab-hero-title"
    >
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_50%_70%_at_22%_40%,rgba(124,58,237,0.16),transparent_70%)]"
        aria-hidden
      />
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_42%_58%_at_82%_32%,rgba(59,130,246,0.12),transparent_70%)]"
        aria-hidden
      />

      <PromptLabContainer>
        <div className="relative overflow-hidden rounded-[18px] border border-[rgba(120,90,255,0.15)] bg-[linear-gradient(135deg,#070b18,#111433)]">
          <div
            className="pointer-events-none absolute inset-x-[10%] bottom-0 h-20 bg-[radial-gradient(ellipse_at_center,rgba(124,58,237,0.22),transparent_70%)] blur-xl"
            aria-hidden
          />
          <div
            className="relative grid min-h-[250px] items-center gap-6 px-6 py-9 sm:px-8 md:h-[260px] md:min-h-[260px] md:grid-cols-[minmax(0,1.05fr)_minmax(0,0.95fr)] md:gap-8 md:px-10 md:py-0 lg:px-12"
            dir="ltr"
          >
            <div className="order-2 max-w-[520px] text-center md:order-1 md:text-left" dir="rtl">
              <p className="text-[13px] font-bold tracking-[0.04em] text-[#A78BFA] sm:text-[14px]">
                {PROMPT_LAB_HERO_EYEBROW}
              </p>
              <h1
                id="prompt-lab-hero-title"
                className="mt-2 bg-gradient-to-l from-[#9b45ff] to-[#5b8cff] bg-clip-text text-[36px] font-extrabold leading-[1.25] tracking-tight text-transparent sm:text-[42px] md:text-[48px]"
              >
                {PROMPT_LAB_HERO_TITLE}
              </h1>
              <p className="mt-3 max-w-[450px] text-[14px] leading-7 text-[#94A3B8] sm:text-[15px]">
                {PROMPT_LAB_HERO_SUBTITLE}
              </p>

              <div className="mt-5 flex flex-col items-stretch gap-3 sm:flex-row sm:items-center sm:justify-center md:justify-start">
                <Link
                  href="/write/prompts/new"
                  className="focus-ring inline-flex h-11 items-center justify-center gap-2 rounded-xl bg-gradient-to-l from-[#7C3AED] to-[#6D28D9] px-6 text-[14px] font-bold text-white no-underline shadow-[0_0_18px_rgba(124,58,237,0.35)] transition hover:brightness-110"
                >
                  <PlusIcon className="h-4 w-4 shrink-0" />
                  پرامپت جدید
                </Link>
                <Link
                  href="#prompt-lab-catalog"
                  className="focus-ring inline-flex h-11 items-center justify-center gap-2 rounded-xl border border-white/[0.12] bg-[#0F1626]/80 px-6 text-[14px] font-bold text-[#E5E7EB] no-underline backdrop-blur-sm transition hover:border-[rgba(168,85,247,0.4)] hover:text-white"
                >
                  <FlaskIcon className="h-4 w-4 shrink-0 text-[#A78BFA]" />
                  ورود Prompt Lab
                </Link>
              </div>
            </div>

            <div className="order-1 flex items-center justify-center md:order-2 md:justify-end">
              <div className="relative flex h-[190px] w-full max-w-[300px] items-center justify-center sm:h-[220px] sm:max-w-[340px] md:h-[240px] md:max-w-[380px]">
                <span
                  className="pointer-events-none absolute inset-0 rounded-full bg-[radial-gradient(circle,rgba(168,85,247,0.35),rgba(59,130,246,0.12)_52%,transparent_74%)] blur-2xl"
                  aria-hidden
                />
                <img
                  src={PROMPT_LAB_HERO_IMAGE_SRC}
                  alt=""
                  width={380}
                  height={380}
                  loading="eager"
                  fetchPriority="high"
                  decoding="async"
                  className="relative h-full w-full object-contain drop-shadow-[0_18px_46px_rgba(124,58,237,0.45)]"
                />
              </div>
            </div>
          </div>
        </div>
      </PromptLabContainer>
    </section>
  );
}
