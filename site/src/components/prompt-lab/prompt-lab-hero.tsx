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

/** Reference hero: 280px · flask 420px without black square. */
export function PromptLabHero() {
  return (
    <section
      className="relative overflow-hidden bg-[#070b18] pb-6 pt-5"
      aria-labelledby="prompt-lab-hero-title"
    >
      <PromptLabContainer>
        <div className="relative overflow-hidden rounded-[20px] border border-[rgba(139,92,246,0.2)] bg-[linear-gradient(135deg,#070b18,#111433)]">
          <div
            className="pointer-events-none absolute inset-x-[12%] bottom-0 h-16 bg-[radial-gradient(ellipse_at_center,rgba(124,58,237,0.18),transparent_70%)] blur-xl"
            aria-hidden
          />
          <div
            className="relative grid min-h-[260px] items-center gap-4 px-6 py-6 sm:px-8 md:h-[280px] md:min-h-[280px] md:grid-cols-[minmax(0,1fr)_420px] md:gap-6 md:px-10 md:py-0 lg:px-12"
            dir="ltr"
          >
            <div className="order-2 max-w-[500px] text-center md:order-1 md:text-left" dir="rtl">
              <p className="text-[13px] font-bold tracking-[0.04em] text-[#A78BFA]">
                {PROMPT_LAB_HERO_EYEBROW}
              </p>
              <h1
                id="prompt-lab-hero-title"
                className="mt-1.5 bg-gradient-to-l from-[#9b45ff] to-[#5b8cff] bg-clip-text text-[34px] font-extrabold leading-[1.2] tracking-tight text-transparent sm:text-[42px] md:text-[48px]"
              >
                {PROMPT_LAB_HERO_TITLE}
              </h1>
              <p className="mt-2.5 max-w-[450px] text-[14px] leading-7 text-[#94A3B8]">
                {PROMPT_LAB_HERO_SUBTITLE}
              </p>

              <div className="mt-4 flex flex-col items-stretch gap-2.5 sm:flex-row sm:items-center sm:justify-center md:justify-start">
                <Link
                  href="/write/prompts/new"
                  className="focus-ring inline-flex h-11 items-center justify-center gap-2 rounded-xl bg-gradient-to-l from-[#7C3AED] to-[#6D28D9] px-5 text-[14px] font-bold text-white no-underline shadow-[0_0_18px_rgba(124,58,237,0.35)] transition hover:brightness-110"
                >
                  <PlusIcon className="h-4 w-4 shrink-0" />
                  پرامپت جدید
                </Link>
                <Link
                  href="#prompt-lab-catalog"
                  className="focus-ring inline-flex h-11 items-center justify-center gap-2 rounded-xl border border-white/[0.12] bg-[#0F1626]/85 px-5 text-[14px] font-bold text-[#E5E7EB] no-underline backdrop-blur-sm transition hover:border-[rgba(168,85,247,0.4)] hover:text-white"
                >
                  <FlaskIcon className="h-4 w-4 shrink-0 text-[#A78BFA]" />
                  ورود Prompt Lab
                </Link>
              </div>
            </div>

            <div className="order-1 flex items-center justify-center md:order-2 md:justify-end">
              <div className="relative flex h-[200px] w-full max-w-[320px] items-center justify-center md:h-[250px] md:w-[420px] md:max-w-[420px]">
                <span
                  className="pointer-events-none absolute inset-[12%] rounded-full bg-[radial-gradient(circle,rgba(168,85,247,0.28),transparent_70%)] blur-2xl"
                  aria-hidden
                />
                <img
                  src={PROMPT_LAB_HERO_IMAGE_SRC}
                  alt=""
                  width={420}
                  height={420}
                  loading="eager"
                  fetchPriority="high"
                  decoding="async"
                  className="relative h-full w-full object-contain object-center mix-blend-screen drop-shadow-[0_16px_40px_rgba(124,58,237,0.4)]"
                />
              </div>
            </div>
          </div>
        </div>
      </PromptLabContainer>
    </section>
  );
}
