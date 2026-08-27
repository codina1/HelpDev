import { PublicContainer } from "@/components/ui/public/v2/public-container";

/** Existing 3D news illustration used by the news hero. */
export const NEWS_HERO_IMAGE_SRC = "/home/icon-news.png";

/**
 * News page hero — RTL, two-column desktop layout and stacked mobile layout.
 */
export function NewsHero() {
  return (
    <section
      className="relative overflow-hidden bg-[#050816] py-6 sm:py-8 lg:py-10"
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
        <div className="relative overflow-hidden rounded-[24px] border border-white/[0.08] bg-[#0B1224]/75 px-6 py-8 shadow-[0_0_45px_rgba(124,58,237,0.16)] backdrop-blur-xl sm:px-10 sm:py-10 lg:px-14 lg:py-12">
          <div
            className="pointer-events-none absolute -start-24 -top-28 h-72 w-72 rounded-full bg-[rgba(124,58,237,0.16)] blur-3xl"
            aria-hidden
          />
          <div
            className="pointer-events-none absolute -end-20 -bottom-32 h-80 w-80 rounded-full bg-[rgba(37,99,235,0.1)] blur-3xl"
            aria-hidden
          />

          <div className="relative grid items-center gap-8 lg:grid-cols-2 lg:gap-12">
            <div className="order-1 flex min-h-48 items-center justify-center sm:min-h-60 lg:min-h-64">
              <div className="relative flex h-48 w-full max-w-[360px] items-center justify-center sm:h-60 lg:h-64">
                <span
                  className="pointer-events-none absolute inset-[15%] rounded-full bg-[rgba(124,58,237,0.24)] blur-3xl"
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

            <div className="order-2 max-w-xl text-start">
              <p className="mb-3 text-[13px] font-bold tracking-wide text-[#A78BFA]">اخبار</p>
              <h1
                id="news-hero-heading"
                className="text-[2rem] font-extrabold leading-[1.35] tracking-tight text-white sm:text-[2.55rem] lg:text-[3rem]"
              >
                آخرین{" "}
                <span className="bg-gradient-to-l from-[#A855F7] via-[#8B5CF6] to-[#3B82F6] bg-clip-text text-transparent">
                  اخبار فناوری
                </span>
              </h1>
              <p className="mt-4 max-w-lg text-[14px] leading-8 text-[#94A3B8] sm:text-[15px]">
                جدیدترین اتفاقات دنیای برنامه‌نویسی،
                <br />
                هوش مصنوعی و ابزارهای توسعه را در HelpDev بخوانید.
              </p>
              <span
                className="mt-6 block h-px w-24 bg-gradient-to-l from-[#A855F7] to-transparent"
                aria-hidden
              />
            </div>
          </div>
        </div>
      </PublicContainer>
    </section>
  );
}
