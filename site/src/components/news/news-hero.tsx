import { PublicContainer } from "@/components/ui/public/v2/public-container";

/** Existing 3D news illustration used by the news hero. */
export const NEWS_HERO_IMAGE_SRC = "/home/icon-news.png";

/**
 * News page hero — RTL, two-column desktop layout and stacked mobile layout.
 * Fluid across monitor sizes: no hard clip height on mid screens.
 */
export function NewsHero() {
  return (
    <section
      className="relative overflow-hidden bg-[#050816] py-4 sm:py-5 lg:py-6"
      aria-labelledby="news-hero-heading"
      dir="rtl"
    >
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_55%_75%_at_22%_35%,rgba(124,58,237,0.18),transparent_68%)]"
        aria-hidden
      />
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_45%_60%_at_82%_0%,rgba(37,99,235,0.12),transparent_70%)]"
        aria-hidden
      />

      <PublicContainer size="wide">
        <div className="relative overflow-hidden rounded-[20px] border border-white/[0.08] bg-[#0B1224]/75 px-5 py-6 shadow-[0_0_45px_rgba(124,58,237,0.16)] backdrop-blur-xl sm:rounded-[24px] sm:px-8 sm:py-7 lg:min-h-[320px] lg:px-10 lg:py-6 xl:min-h-[360px] xl:px-12">
          <div
            className="pointer-events-none absolute -start-24 -top-28 h-72 w-72 rounded-full bg-[rgba(124,58,237,0.16)] blur-3xl"
            aria-hidden
          />
          <div
            className="pointer-events-none absolute -end-20 -bottom-32 h-80 w-80 rounded-full bg-[rgba(37,99,235,0.1)] blur-3xl"
            aria-hidden
          />

          <div className="relative grid h-full items-center gap-5 sm:gap-6 lg:grid-cols-2 lg:gap-8 xl:gap-10">
            <div className="order-1 flex items-center justify-center lg:min-h-0">
              <div className="relative flex h-40 w-full max-w-[280px] items-center justify-center sm:h-52 sm:max-w-[340px] lg:h-[260px] lg:max-w-[400px] xl:h-[300px] xl:max-w-[440px]">
                <span
                  className="pointer-events-none absolute inset-[8%] rounded-full bg-[radial-gradient(circle,rgba(124,58,237,0.32),rgba(37,99,235,0.14)_42%,transparent_72%)] blur-2xl"
                  aria-hidden
                />
                <img
                  src={NEWS_HERO_IMAGE_SRC}
                  alt=""
                  width={256}
                  height={256}
                  decoding="async"
                  className="relative h-full w-full object-contain drop-shadow-[0_18px_38px_rgba(124,58,237,0.5)]"
                />
              </div>
            </div>

            <div className="order-2 max-w-xl text-center lg:text-start xl:-translate-x-2">
              <p className="mb-2 text-[12px] font-bold tracking-wide text-[#A78BFA] sm:text-[13px]">
                اخبار
              </p>
              <h1
                id="news-hero-heading"
                className="text-[1.75rem] font-extrabold leading-[1.3] tracking-tight text-white sm:text-[2.25rem] lg:text-[2.75rem] xl:text-[3.1rem]"
              >
                آخرین{" "}
                <span className="bg-gradient-to-l from-[#A855F7] via-[#8B5CF6] to-[#3B82F6] bg-clip-text text-transparent">
                  اخبار فناوری
                </span>
              </h1>
              <p className="mt-3 max-w-lg text-[13px] leading-7 text-[#94A3B8] sm:text-[14px] lg:mx-0 lg:text-[15px]">
                جدیدترین اتفاقات دنیای برنامه‌نویسی، هوش مصنوعی و ابزارهای توسعه را در HelpDev بخوانید.
              </p>
              <span
                className="mx-auto mt-4 block h-px w-24 bg-gradient-to-l from-[#A855F7] to-transparent lg:mx-0"
                aria-hidden
              />
            </div>
          </div>
        </div>
      </PublicContainer>
    </section>
  );
}
