import { PublicContainer } from "@/components/ui/public/v2/public-container";

/** Transparent neon NEWS tablet for the news hero. */
export const NEWS_HERO_IMAGE_SRC = "/home/icon-news-neon.png";

/**
 * News page hero — full-width container, 330px height on desktop.
 * Text on the left, larger neon NEWS icon on the right.
 */
export function NewsHero() {
  return (
    <section
      className="relative overflow-hidden bg-[#050816] pb-4 pt-5 sm:pb-5 sm:pt-6"
      aria-labelledby="news-hero-heading"
    >
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_55%_70%_at_18%_40%,rgba(124,58,237,0.22),transparent_68%)]"
        aria-hidden
      />
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_42%_58%_at_88%_12%,rgba(37,99,235,0.16),transparent_70%)]"
        aria-hidden
      />

      <PublicContainer size="wide">
        <div className="relative mx-auto w-full overflow-hidden rounded-[24px] border border-white/[0.08] bg-[#0B1224] shadow-[0_0_56px_rgba(124,58,237,0.2)]">
          <div
            className="pointer-events-none absolute -start-24 -top-28 h-80 w-80 rounded-full bg-[rgba(124,58,237,0.2)] blur-3xl"
            aria-hidden
          />
          <div
            className="pointer-events-none absolute -end-10 bottom-[-35%] h-80 w-80 rounded-full bg-[rgba(59,130,246,0.14)] blur-3xl"
            aria-hidden
          />
          <div
            className="pointer-events-none absolute inset-x-[12%] bottom-0 h-24 bg-[radial-gradient(ellipse_at_center,rgba(124,58,237,0.28),transparent_70%)] blur-2xl"
            aria-hidden
          />

          <div
            className="relative grid min-h-[240px] items-center gap-5 px-5 py-6 sm:min-h-[280px] sm:gap-8 sm:px-8 sm:py-7 md:h-[330px] md:min-h-[330px] md:grid-cols-2 md:gap-10 md:px-10 md:py-0 lg:px-12 xl:px-14"
            dir="ltr"
          >
            {/* Text — left */}
            <div className="order-2 max-w-xl text-center md:order-1 md:text-left" dir="rtl">
              <h1
                id="news-hero-heading"
                className="text-[1.85rem] font-extrabold leading-[1.25] tracking-tight text-white sm:text-[2.35rem] md:text-[2.55rem] lg:text-[2.85rem]"
              >
                <span className="bg-gradient-to-l from-[#C084FC] via-[#A855F7] to-[#7C3AED] bg-clip-text text-transparent">
                  آخرین
                </span>{" "}
                اخبار فناوری
              </h1>
              <p className="mt-3.5 max-w-md text-[13px] leading-7 text-[#94A3B8] sm:mt-4 sm:text-[15px] sm:leading-8 md:mx-0">
                جدیدترین اتفاقات دنیای برنامه‌نویسی، هوش مصنوعی و ابزارهای توسعه را در HelpDev بخوانید.
              </p>
            </div>

            {/* Icon — right, larger */}
            <div className="order-1 flex items-center justify-center md:order-2 md:justify-end">
              <div className="relative flex h-[168px] w-full max-w-[300px] items-center justify-center sm:h-[210px] sm:max-w-[360px] md:h-[270px] md:max-w-[420px] lg:h-[290px] lg:max-w-[460px]">
                <span
                  className="pointer-events-none absolute inset-[-4%] rounded-full bg-[radial-gradient(circle,rgba(168,85,247,0.48),rgba(59,130,246,0.18)_48%,transparent_72%)] blur-2xl"
                  aria-hidden
                />
                <img
                  src={NEWS_HERO_IMAGE_SRC}
                  alt=""
                  width={460}
                  height={358}
                  decoding="async"
                  className="relative h-full w-full scale-110 object-contain drop-shadow-[0_16px_48px_rgba(124,58,237,0.6)] md:scale-125"
                />
              </div>
            </div>
          </div>
        </div>
      </PublicContainer>
    </section>
  );
}
