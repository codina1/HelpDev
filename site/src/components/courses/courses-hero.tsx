import Link from "next/link";
import { CoursesContainer } from "@/components/courses/courses-container";

export const COURSES_HERO_IMAGE_SRC = "/courses/hero-learning.png";

function GridIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <rect x="3.5" y="3.5" width="7" height="7" rx="2" stroke="currentColor" strokeWidth="1.8" />
      <rect x="13.5" y="3.5" width="7" height="7" rx="2" stroke="currentColor" strokeWidth="1.8" />
      <rect x="3.5" y="13.5" width="7" height="7" rx="2" stroke="currentColor" strokeWidth="1.8" />
      <rect x="13.5" y="13.5" width="7" height="7" rx="2" stroke="currentColor" strokeWidth="1.8" />
    </svg>
  );
}

function PathIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <circle cx="6" cy="6" r="2.6" stroke="currentColor" strokeWidth="1.8" />
      <circle cx="18" cy="18" r="2.6" stroke="currentColor" strokeWidth="1.8" />
      <path d="M8.6 6H14a3.4 3.4 0 0 1 0 6.8h-4A3.4 3.4 0 0 0 10 18h5.4" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  );
}

/** Reference hero: text right (RTL) / 3D graduation illustration left. */
export function CoursesHero() {
  return (
    <section className="relative overflow-hidden bg-[#030712] pb-3 pt-5" aria-labelledby="courses-hero-heading">
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_50%_70%_at_20%_40%,rgba(124,58,237,0.18),transparent_70%)]"
        aria-hidden
      />
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_40%_55%_at_85%_30%,rgba(59,130,246,0.12),transparent_70%)]"
        aria-hidden
      />

      <CoursesContainer>
        <div className="relative overflow-hidden rounded-[18px] border border-white/[0.07] bg-gradient-to-bl from-[#0D1226] via-[#0A0F20] to-[#100A22]">
          <div
            className="pointer-events-none absolute inset-x-[10%] bottom-0 h-20 bg-[radial-gradient(ellipse_at_center,rgba(124,58,237,0.2),transparent_70%)] blur-xl"
            aria-hidden
          />
          <div
            className="relative grid min-h-[260px] items-center gap-6 px-6 py-9 sm:px-8 md:h-[315px] md:min-h-[315px] md:grid-cols-2 md:gap-10 md:px-12 md:py-0 lg:px-16"
            dir="ltr"
          >
            <div className="order-2 max-w-xl text-center md:order-1 md:text-left" dir="rtl">
              <p className="text-[13px] font-bold tracking-[0.06em] text-[#A78BFA] sm:text-[14px]">
                دوره‌ها و آموزش‌ها
              </p>
              <h1
                id="courses-hero-heading"
                className="mt-2.5 text-[30px] font-extrabold leading-[1.32] tracking-tight text-white sm:text-[36px] md:text-[42px] lg:text-[46px]"
              >
                یادگیری{" "}
                <span className="bg-gradient-to-l from-[#C084FC] via-[#A855F7] to-[#3B82F6] bg-clip-text text-transparent">
                  مهارت‌های آینده
                </span>
              </h1>
              <p className="mt-4 max-w-[560px] text-[15px] leading-9 text-[#94A3B8] sm:text-[16px]">
                دوره‌های کاربردی، برنامه‌نویسی، ابزارها و مهارت‌های موردنیاز توسعه‌دهندگان را از مقدماتی تا پیشرفته یاد بگیرید.
              </p>

              <div className="mt-6 flex flex-col items-stretch gap-3 sm:flex-row sm:items-center sm:justify-center md:justify-start">
                <Link
                  href="#courses-grid"
                  className="focus-ring inline-flex h-12 items-center justify-center gap-2 rounded-xl bg-gradient-to-l from-[#7C3AED] to-[#6D28D9] px-6 text-[14px] font-bold text-white no-underline shadow-[0_0_18px_rgba(124,58,237,0.3)] transition hover:brightness-110"
                >
                  <GridIcon className="h-4 w-4 shrink-0" />
                  مشاهده همه دوره‌ها
                </Link>
                <Link
                  href="/roadmap"
                  className="focus-ring inline-flex h-12 items-center justify-center gap-2 rounded-xl border border-white/[0.1] bg-[#111827] px-6 text-[14px] font-bold text-[#E5E7EB] no-underline transition hover:border-[rgba(168,85,247,0.4)] hover:text-white"
                >
                  <PathIcon className="h-4 w-4 shrink-0" />
                  مسیرهای یادگیری
                </Link>
              </div>
            </div>

            <div className="order-1 flex items-center justify-center md:order-2 md:justify-end">
              <div className="relative flex h-[180px] w-full max-w-[340px] items-center justify-center sm:h-[220px] sm:max-w-[440px] md:h-[290px] md:max-w-[580px]">
                <span
                  className="pointer-events-none absolute inset-0 rounded-full bg-[radial-gradient(circle,rgba(124,58,237,0.35),rgba(59,130,246,0.12)_52%,transparent_74%)] blur-2xl"
                  aria-hidden
                />
                <img
                  src={COURSES_HERO_IMAGE_SRC}
                  alt=""
                  width={460}
                  height={230}
                  loading="eager"
                  fetchPriority="high"
                  decoding="async"
                  className="relative h-full w-full scale-105 object-contain drop-shadow-[0_18px_46px_rgba(124,58,237,0.45)] md:scale-110"
                />
              </div>
            </div>
          </div>
        </div>
      </CoursesContainer>
    </section>
  );
}
