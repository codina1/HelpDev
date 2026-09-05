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

/** Articles hero — ~236px · full-width glass · 50/50 text+book. */
export function ArticlesHero() {
  return (
    <section
      className="relative overflow-hidden bg-[#070b18] pb-8 pt-5"
      aria-labelledby="articles-hero-title"
    >
      <ArticlesContainer>
        <div className="relative overflow-hidden rounded-[20px] border border-[rgba(139,92,246,0.22)] bg-[linear-gradient(135deg,#070b18_0%,#111433_55%,#0c1029_100%)] shadow-[0_0_48px_rgba(124,58,237,0.12)]">
          <div
            className="pointer-events-none absolute inset-x-[10%] bottom-0 h-16 bg-[radial-gradient(ellipse_at_center,rgba(124,58,237,0.22),transparent_70%)] blur-2xl"
            aria-hidden
          />

          <div
            className="relative grid items-center gap-4 px-6 py-5 sm:px-8 md:h-[236px] md:grid-cols-[minmax(0,1.05fr)_minmax(280px,0.95fr)] md:gap-6 md:px-10 md:py-0 lg:px-12"
            dir="ltr"
          >
            <div className="order-2 min-w-0 text-center md:order-1 md:text-left" dir="rtl">
              <p className="text-[12px] font-bold tracking-[0.05em] text-[#A78BFA]">
                مرکز دانش توسعه‌دهندگان
              </p>
              <h1
                id="articles-hero-title"
                className="mt-1.5 text-[26px] font-extrabold leading-[1.3] tracking-tight text-white sm:text-[32px] md:text-[36px] lg:text-[38px]"
              >
                مقالات{" "}
                <span className="bg-gradient-to-l from-[#9b45ff] to-[#5b8cff] bg-clip-text text-transparent">
                  تخصصی
                </span>{" "}
                توسعه نرم‌افزار
              </h1>
              <p className="mt-2 max-w-[520px] text-[13px] leading-6 text-[#94A3B8] sm:text-[14px] sm:leading-7">
                آخرین آموزش‌ها، بررسی ابزارها و تحلیل تکنولوژی‌های روز برنامه‌نویسی، هوش مصنوعی و
                توسعه نرم‌افزار
              </p>

              <div className="mt-4 flex flex-col items-stretch gap-2.5 sm:flex-row sm:items-center sm:justify-center md:justify-start">
                <Link
                  href="#articles-catalog"
                  className="focus-ring inline-flex h-10 items-center justify-center gap-2 rounded-xl bg-gradient-to-l from-[#7C3AED] to-[#6D28D9] px-5 text-[13.5px] font-bold text-white no-underline shadow-[0_0_20px_rgba(124,58,237,0.35)] transition hover:brightness-110"
                >
                  <DocIcon className="h-4 w-4 shrink-0" />
                  مشاهده مقالات
                </Link>
                <Link
                  href="#articles-categories"
                  className="focus-ring inline-flex h-10 items-center justify-center gap-2 rounded-xl border border-white/[0.12] bg-[#0F1626]/85 px-5 text-[13.5px] font-bold text-[#E5E7EB] no-underline backdrop-blur-sm transition hover:border-[rgba(168,85,247,0.4)] hover:text-white"
                >
                  <GridIcon className="h-4 w-4 shrink-0 text-[#A78BFA]" />
                  دسته‌بندی‌ها
                </Link>
              </div>
            </div>

            <div className="order-1 flex h-[160px] items-center justify-center md:order-2 md:h-full md:justify-end">
              <div className="relative h-full w-full max-w-[420px] md:max-w-none">
                <span
                  className="pointer-events-none absolute inset-[6%] rounded-full bg-[radial-gradient(circle,rgba(168,85,247,0.32),transparent_65%)] blur-3xl"
                  aria-hidden
                />
                <img
                  src={ARTICLES_HERO_IMAGE_SRC}
                  alt=""
                  width={480}
                  height={280}
                  loading="eager"
                  fetchPriority="high"
                  decoding="async"
                  className="relative h-full w-full object-contain object-center mix-blend-screen drop-shadow-[0_16px_40px_rgba(124,58,237,0.4)]"
                />
              </div>
            </div>
          </div>
        </div>
      </ArticlesContainer>
    </section>
  );
}
