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

/** Articles hero — knowledge center copy · neon digital book. */
export function ArticlesHero() {
  return (
    <section
      className="relative overflow-hidden bg-[#070b18] pb-6 pt-5"
      aria-labelledby="articles-hero-title"
    >
      <ArticlesContainer>
        <div className="relative overflow-hidden rounded-[20px] border border-[rgba(139,92,246,0.2)] bg-[linear-gradient(135deg,#070b18,#111433)]">
          <div
            className="pointer-events-none absolute inset-x-[12%] bottom-0 h-16 bg-[radial-gradient(ellipse_at_center,rgba(124,58,237,0.2),transparent_70%)] blur-xl"
            aria-hidden
          />
          <div
            className="relative grid min-h-[260px] items-center gap-5 px-6 py-7 sm:px-8 md:h-[280px] md:min-h-[280px] md:grid-cols-[minmax(0,1fr)_420px] md:gap-6 md:px-10 md:py-0 lg:px-12"
            dir="ltr"
          >
            <div className="order-2 max-w-[540px] text-center md:order-1 md:text-left" dir="rtl">
              <p className="text-[13px] font-bold tracking-[0.04em] text-[#A78BFA]">
                مرکز دانش توسعه‌دهندگان
              </p>
              <h1
                id="articles-hero-title"
                className="mt-2 text-[30px] font-extrabold leading-[1.35] tracking-tight text-white sm:text-[38px] md:text-[42px]"
              >
                مقالات{" "}
                <span className="bg-gradient-to-l from-[#9b45ff] to-[#5b8cff] bg-clip-text text-transparent">
                  تخصصی
                </span>{" "}
                توسعه نرم‌افزار
              </h1>
              <p className="mt-3 max-w-[480px] text-[14px] leading-7 text-[#94A3B8] sm:text-[15px]">
                آخرین آموزش‌ها، بررسی ابزارها و تحلیل تکنولوژی‌های روز برنامه‌نویسی، هوش مصنوعی و
                توسعه نرم‌افزار
              </p>

              <div className="mt-5 flex flex-col items-stretch gap-2.5 sm:flex-row sm:items-center sm:justify-center md:justify-start">
                <Link
                  href="#articles-catalog"
                  className="focus-ring inline-flex h-11 items-center justify-center gap-2 rounded-xl bg-gradient-to-l from-[#7C3AED] to-[#6D28D9] px-5 text-[14px] font-bold text-white no-underline shadow-[0_0_18px_rgba(124,58,237,0.35)] transition hover:brightness-110"
                >
                  <DocIcon className="h-4 w-4 shrink-0" />
                  مشاهده مقالات
                </Link>
                <Link
                  href="#articles-categories"
                  className="focus-ring inline-flex h-11 items-center justify-center gap-2 rounded-xl border border-white/[0.12] bg-[#0F1626]/85 px-5 text-[14px] font-bold text-[#E5E7EB] no-underline backdrop-blur-sm transition hover:border-[rgba(168,85,247,0.4)] hover:text-white"
                >
                  <GridIcon className="h-4 w-4 shrink-0 text-[#A78BFA]" />
                  دسته‌بندی‌ها
                </Link>
              </div>
            </div>

            <div className="order-1 flex items-center justify-center md:order-2 md:justify-end">
              <div className="relative flex h-[190px] w-full max-w-[300px] items-center justify-center md:h-[250px] md:w-[420px] md:max-w-[420px]">
                <span
                  className="pointer-events-none absolute inset-[10%] rounded-full bg-[radial-gradient(circle,rgba(168,85,247,0.3),transparent_70%)] blur-2xl"
                  aria-hidden
                />
                <img
                  src={ARTICLES_HERO_IMAGE_SRC}
                  alt=""
                  width={420}
                  height={420}
                  loading="eager"
                  fetchPriority="high"
                  decoding="async"
                  className="relative h-full w-full object-contain mix-blend-screen drop-shadow-[0_16px_40px_rgba(124,58,237,0.4)]"
                />
              </div>
            </div>
          </div>
        </div>
      </ArticlesContainer>
    </section>
  );
}
