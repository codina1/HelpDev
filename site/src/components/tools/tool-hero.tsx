import Link from "next/link";
import { ToolsContainer } from "@/components/tools/tools-container";
import { TOOLS_HERO_IMAGE_SRC } from "@/data/tools";

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

function StarIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="currentColor" aria-hidden>
      <path d="M12 3.4 14.4 9l6 .5-4.6 3.9 1.4 5.8L12 16.8 6.8 19.2l1.4-5.8L3.6 9.5l6-.5L12 3.4Z" />
    </svg>
  );
}

/** Tools hero — badge · title · CTAs · neon toolbox illustration. */
export function ToolHero() {
  return (
    <section
      className="relative overflow-hidden bg-[#070b18] pb-6 pt-5"
      aria-labelledby="tools-hero-title"
    >
      <ToolsContainer>
        <div className="relative overflow-hidden rounded-[20px] border border-[rgba(139,92,246,0.2)] bg-[linear-gradient(135deg,#070b18,#111433)]">
          <div
            className="pointer-events-none absolute inset-x-[12%] bottom-0 h-16 bg-[radial-gradient(ellipse_at_center,rgba(124,58,237,0.2),transparent_70%)] blur-xl"
            aria-hidden
          />
          <div
            className="relative grid min-h-[260px] items-center gap-5 px-6 py-7 sm:px-8 md:h-[280px] md:min-h-[280px] md:grid-cols-[minmax(0,1fr)_420px] md:gap-6 md:px-10 md:py-0 lg:px-12"
            dir="ltr"
          >
            <div className="order-2 max-w-[520px] text-center md:order-1 md:text-left" dir="rtl">
              <p className="inline-flex rounded-full border border-[rgba(168,85,247,0.35)] bg-[rgba(124,58,237,0.12)] px-3 py-1 text-[12px] font-bold text-[#C4B5FD]">
                ابزارها
              </p>
              <h1
                id="tools-hero-title"
                className="mt-3 text-[32px] font-extrabold leading-[1.25] tracking-tight text-white sm:text-[40px] md:text-[46px]"
              >
                بهترین{" "}
                <span className="bg-gradient-to-l from-[#9b45ff] to-[#5b8cff] bg-clip-text text-transparent">
                  ابزارهای
                </span>{" "}
                توسعه
              </h1>
              <p className="mt-3 max-w-[480px] text-[14px] leading-7 text-[#94A3B8] sm:text-[15px]">
                مجموعه‌ای منتخب از بهترین ابزارها و سرویس‌هایی که به توسعه‌دهندگان کمک می‌کنند
                سریع‌تر، هوشمندتر و با کیفیت‌تر کار کنند.
              </p>

              <div className="mt-5 flex flex-col items-stretch gap-2.5 sm:flex-row sm:items-center sm:justify-center md:justify-start">
                <Link
                  href="#tools-catalog"
                  className="focus-ring inline-flex h-11 items-center justify-center gap-2 rounded-xl bg-gradient-to-l from-[#7C3AED] to-[#6D28D9] px-5 text-[14px] font-bold text-white no-underline shadow-[0_0_18px_rgba(124,58,237,0.35)] transition hover:brightness-110"
                >
                  <GridIcon className="h-4 w-4 shrink-0" />
                  همه ابزارها
                </Link>
                <Link
                  href="#tools-catalog"
                  className="focus-ring inline-flex h-11 items-center justify-center gap-2 rounded-xl border border-white/[0.12] bg-[#0F1626]/85 px-5 text-[14px] font-bold text-[#E5E7EB] no-underline backdrop-blur-sm transition hover:border-[rgba(168,85,247,0.4)] hover:text-white"
                >
                  <StarIcon className="h-4 w-4 shrink-0 text-[#FBBF24]" />
                  ابزارهای محبوب
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
                  src={TOOLS_HERO_IMAGE_SRC}
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
      </ToolsContainer>
    </section>
  );
}
