import Link from "next/link";
import { ArticlesContainer } from "@/components/articles/articles-container";
import { ARTICLES_HERO_IMAGE_SRC } from "@/data/articles";

function DocIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path d="M6.5 3.5h7.2L18.5 8v12.5h-12V3.5Z" stroke="currentColor" strokeWidth="1.7" strokeLinejoin="round" />
      <path d="M13.4 3.6V8.3h4.8M9.2 12.4h6M9.2 15.8h4" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" />
    </svg>
  );
}

function GridIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <rect x="3.5" y="3.5" width="7" height="7" rx="1.6" stroke="currentColor" strokeWidth="1.7" />
      <rect x="13.5" y="3.5" width="7" height="7" rx="1.6" stroke="currentColor" strokeWidth="1.7" />
      <rect x="3.5" y="13.5" width="7" height="7" rx="1.6" stroke="currentColor" strokeWidth="1.7" />
      <rect x="13.5" y="13.5" width="7" height="7" rx="1.6" stroke="currentColor" strokeWidth="1.7" />
    </svg>
  );
}

/** Articles hero — knowledge center copy · neon digital book — 280px tall. */
export function ArticlesHero() {
  return (
    <section
      className="relative overflow-hidden bg-[#070b18] pb-10 pt-6"
      aria-labelledby="articles-hero-title"
    >
      <ArticlesContainer>
        <div className="relative overflow-hidden rounded-[22px] border border-[rgba(139,92,246,0.22)] bg-[linear-gradient(135deg,#070b18_0%,#111433_50%,#0c1029_100%)] shadow-[0_0_60px_rgba(124,58,237,0.12)]">
          {/* bottom glow */}
          <div
            className="pointer-events-none absolute inset-x-[8%] bottom-0 h-24 bg-[radial-gradient(ellipse_at_center,rgba(124,58,237,0.25),transparent_70%)] blur-2xl"
            aria-hidden
          />

          <div
            className="relative grid min-h-[260px] items-center gap-6 px-7 py-8 sm:px-10 md:h-[280px] md:min-h-[280px] md:grid-cols-[minmax(0,1fr)_440px] md:gap-8 md:px-12 md:py-0 lg:px-14"
            dir="ltr"
          >
            {/* Text — left side (LTR layout, right side visually in RTL) */}
            <div className="order-2 max-w-[560px] text-center md:order-1 md:text-left" dir="rtl">
              <p className="text-[13px] font-bold tracking-[0.06em] text-[#A78BFA]">
                مرکز دانش توسعه‌دهندگان
              </p>
              <h1
                id="articles-hero-title"
                className="mt-3 text-[32px] font-extrabold leading-[1.3] tracking-tight text-white sm:text-[40px] md:text-[46px]"
              >
                مقالات{" "}
                <span className="bg-gradient-to-l from-[#9b45ff] to-[#5b8cff] bg-clip-text text-transparent">
                  تخصصی
                </span>{" "}
                توسعه نرم‌افزار
              </h1>
              <p className="mt-4 max-w-[500px] text-[14.5px] leading-[1.9] text-[#94A3B8] sm:text-[15.5px]">
                آخرین آموزش‌ها، بررسی ابزارها و تحلیل تکنولوژی‌های روز برنامه‌نویسی، هوش مصنوعی و
                توسعه نرم‌افزار
              </p>

              <div className="mt-6 flex flex-col items-stretch gap-3 sm:flex-row sm:items-center sm:justify-center md:justify-start">
                <Link
                  href="#articles-catalog"
                  className="focus-ring inline-flex h-12 items-center justify-center gap-2.5 rounded-2xl bg-gradient-to-l from-[#7C3AED] to-[#6D28D9] px-7 text-[15px] font-bold text-white no-underline shadow-[0_0_24px_rgba(124,58,237,0.4)] transition hover:brightness-110"
                >
                  <DocIcon className="h-[18px] w-[18px] shrink-0" />
                  مشاهده مقالات
                </Link>
                <Link
                  href="#articles-categories"
                  className="focus-ring inline-flex h-12 items-center justify-center gap-2.5 rounded-2xl border border-white/[0.12] bg-[#0F1626]/85 px-7 text-[15px] font-bold text-[#E5E7EB] no-underline backdrop-blur-sm transition hover:border-[rgba(168,85,247,0.4)] hover:text-white"
                >
                  <GridIcon className="h-[18px] w-[18px] shrink-0 text-[#A78BFA]" />
                  دسته‌بندی‌ها
                </Link>
              </div>
            </div>

            {/* Image — right side */}
            <div className="order-1 flex items-center justify-center md:order-2 md:justify-end">
              <div className="relative flex h-[200px] w-full max-w-[340px] items-center justify-center md:h-[260px] md:w-[440px] md:max-w-[440px]">
                <span
                  className="pointer-events-none absolute inset-[8%] rounded-full bg-[radial-gradient(circle,rgba(168,85,247,0.35),transparent_65%)] blur-3xl"
                  aria-hidden
                />
                <img
                  src={ARTICLES_HERO_IMAGE_SRC}
                  alt=""
                  width={440}
                  height={440}
                  loading="eager"
                  fetchPriority="high"
                  decoding="async"
                  className="relative h-full w-full object-contain mix-blend-screen drop-shadow-[0_20px_50px_rgba(124,58,237,0.45)]"
                />
              </div>
            </div>
          </div>
        </div>
      </ArticlesContainer>
    </section>
  );
}
