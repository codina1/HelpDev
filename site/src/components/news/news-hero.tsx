import { PublicContainer } from "@/components/ui/public/v2/public-container";

/** Transparent neon NEWS tablet for the news hero. */
export const NEWS_HERO_IMAGE_SRC = "/home/icon-news-neon.png";

/**
 * News page hero — full-width centered card, fixed 280px height on desktop.
 * Text on the left, neon icon on the right (reference layout).
 */
export function NewsHero() {
  return (
    <section
      className="relative overflow-hidden bg-[#050816] py-5 sm:py-6"
      aria-labelledby="news-hero-heading"
    >
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_55%_70%_at_18%_40%,rgba(124,58,237,0.2),transparent_68%)]"
        aria-hidden
      />
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_40%_55%_at_88%_10%,rgba(37,99,235,0.14),transparent_70%)]"
        aria-hidden
      />

      <PublicContainer size="wide">
        <div className="relative mx-auto w-full overflow-hidden rounded-[24px] border border-white/[0.08] bg-[#0B1224] shadow-[0_0_48px_rgba(124,58,237,0.18)]">
          <div
            className="pointer-events-none absolute -start-24 -top-28 h-72 w-72 rounded-full bg-[rgba(124,58,237,0.18)] blur-3xl"
            aria-hidden
          />
          <div
            className="pointer-events-none absolute -end-16 bottom-[-40%] h-72 w-72 rounded-full bg-[rgba(59,130,246,0.12)] blur-3xl"
            aria-hidden
          />

          <div
            className="relative grid min-h-[220px] items-center gap-4 px-5 py-5 sm:min-h-[240px] sm:gap-6 sm:px-8 sm:py-6 md:grid-cols-2 md:h-[280px] md:min-h-[280px] md:gap-8 md:px-10 md:py-0 lg:px-12"
            dir="ltr"
          >
            {/* Text — left */}
            <div className="order-2 max-w-xl text-center md:order-1 md:text-left" dir="rtl">
              <p className="mb-2 text-[12px] font-bold tracking-[0.08em] text-[#A78BFA] sm:text-[13px]">
                اخبار
              </p>
              <h1
                id="news-hero-heading"
                className="text-[1.7rem] font-extrabold leading-[1.25] tracking-tight text-white sm:text-[2.15rem] md:text-[2.35rem] lg:text-[2.6rem]"
              >
                آخرین{" "}
                <span className="bg-gradient-to-l from-[#A855F7] via-[#8B5CF6] to-[#3B82F6] bg-clip-text text-transparent">
                  اخبار فناوری
                </span>
              </h1>
              <p className="mt-3 max-w-md text-[13px] leading-7 text-[#94A3B8] sm:text-[14px] md:mx-0 md:text-[15px]">
                جدیدترین اتفاقات دنیای برنامه‌نویسی، هوش مصنوعی و ابزارهای توسعه را در HelpDev بخوانید.
              </p>
              <span
                className="mx-auto mt-4 block h-px w-24 bg-gradient-to-l from-[#A855F7] to-transparent md:mx-0"
                aria-hidden
              />
            </div>

            {/* Icon — right */}
            <div className="order-1 flex items-center justify-center md:order-2 md:justify-end">
              <div className="relative flex h-[148px] w-full max-w-[260px] items-center justify-center sm:h-[180px] sm:max-w-[300px] md:h-[220px] md:max-w-[340px] lg:h-[236px] lg:max-w-[380px]">
                <span
                  className="pointer-events-none absolute inset-[2%] rounded-full bg-[radial-gradient(circle,rgba(168,85,247,0.42),rgba(59,130,246,0.16)_45%,transparent_72%)] blur-2xl"
                  aria-hidden
                />
                <img
                  src={NEWS_HERO_IMAGE_SRC}
                  alt=""
                  width={380}
                  height={296}
                  decoding="async"
                  className="relative h-full w-full object-contain drop-shadow-[0_12px_40px_rgba(124,58,237,0.55)]"
                />
              </div>
            </div>
          </div>
        </div>
      </PublicContainer>
    </section>
  );
}
