import { NewsContainer } from "@/components/news/news-container";

export const NEWS_HERO_IMAGE_SRC = "/news/hero-news-icon.png";

/**
 * Reference hero: 330px, text left / NEWS icon right, navy glow.
 */
export function NewsHero() {
  return (
    <section className="relative overflow-hidden bg-[#050816] pt-5 pb-3" aria-labelledby="news-hero-heading">
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_50%_70%_at_20%_40%,rgba(124,58,237,0.2),transparent_70%)]"
        aria-hidden
      />
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_40%_55%_at_85%_30%,rgba(59,130,246,0.14),transparent_70%)]"
        aria-hidden
      />

      <NewsContainer>
        <div className="relative overflow-hidden rounded-[20px] border border-white/[0.08] bg-[#0B1224]">
          <div
            className="pointer-events-none absolute inset-x-[10%] bottom-0 h-20 bg-[radial-gradient(ellipse_at_center,rgba(124,58,237,0.25),transparent_70%)] blur-xl"
            aria-hidden
          />
          <div
            className="relative grid min-h-[240px] items-center gap-6 px-6 py-6 sm:px-8 md:h-[330px] md:min-h-[330px] md:grid-cols-2 md:gap-8 md:px-10 md:py-0 lg:px-12"
            dir="ltr"
          >
            <div className="order-2 max-w-lg text-center md:order-1 md:text-left" dir="rtl">
              <h1
                id="news-hero-heading"
                className="text-[28px] font-extrabold leading-[1.3] tracking-tight text-white sm:text-[34px] md:text-[38px] lg:text-[42px]"
              >
                <span className="bg-gradient-to-l from-[#C084FC] via-[#A855F7] to-[#7C3AED] bg-clip-text text-transparent">
                  آخرین
                </span>{" "}
                اخبار فناوری
              </h1>
              <p className="mt-4 text-[14px] leading-8 text-[#94A3B8] sm:text-[15px]">
                جدیدترین اتفاقات دنیای برنامه‌نویسی، هوش مصنوعی و ابزارهای توسعه را در HelpDev بخوانید.
              </p>
            </div>

            <div className="order-1 flex items-center justify-center md:order-2 md:justify-end">
              <div className="relative flex h-[180px] w-full max-w-[320px] items-center justify-center sm:h-[220px] sm:max-w-[380px] md:h-[280px] md:max-w-[440px]">
                <span
                  className="pointer-events-none absolute inset-0 rounded-full bg-[radial-gradient(circle,rgba(168,85,247,0.45),rgba(59,130,246,0.15)_50%,transparent_72%)] blur-2xl"
                  aria-hidden
                />
                <img
                  src={NEWS_HERO_IMAGE_SRC}
                  alt=""
                  width={440}
                  height={340}
                  decoding="async"
                  className="relative h-full w-full scale-110 object-contain drop-shadow-[0_18px_50px_rgba(124,58,237,0.55)] md:scale-[1.2]"
                />
              </div>
            </div>
          </div>
        </div>
      </NewsContainer>
    </section>
  );
}
