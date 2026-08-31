import Link from "next/link";
import { RoadmapContainer } from "@/components/roadmap/roadmap-container";

export const ROADMAP_GUIDE_IMAGE_SRC = "/roadmap/guide-map.png";

function BookIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path d="M5 6.5A2.5 2.5 0 0 1 7.5 4H19v14.5H7.5A2.5 2.5 0 0 0 5 21V6.5Z" stroke="currentColor" strokeWidth="1.7" strokeLinejoin="round" />
      <path d="M5 18.5h14" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" />
    </svg>
  );
}

/** Guide card: map illustration left, copy and CTA right (RTL). */
export function RoadmapGuide() {
  return (
    <section id="roadmap-guide" className="bg-[#030713] pb-10 pt-2" dir="rtl" aria-labelledby="roadmap-guide-heading">
      <RoadmapContainer>
        <div className="relative overflow-hidden rounded-[18px] border border-white/[0.07] bg-gradient-to-l from-[#0C1226] via-[#090E20] to-[#0E0A20]">
          <div
            className="pointer-events-none absolute inset-y-0 left-0 w-1/2 bg-[radial-gradient(ellipse_at_left,rgba(124,58,237,0.14),transparent_70%)]"
            aria-hidden
          />
          <div className="relative grid items-center gap-6 px-6 py-7 sm:px-8 md:grid-cols-[280px_minmax(0,1fr)] md:gap-10 md:px-10">
            <div className="flex items-center justify-center">
              <img
                src={ROADMAP_GUIDE_IMAGE_SRC}
                alt=""
                width={280}
                height={150}
                loading="lazy"
                decoding="async"
                className="h-auto w-full max-w-[260px] object-contain drop-shadow-[0_14px_34px_rgba(99,102,241,0.35)]"
              />
            </div>

            <div className="text-center md:text-right">
              <h2 id="roadmap-guide-heading" className="text-[17px] font-extrabold text-white sm:text-[19px]">
                چطور از Roadmap استفاده کنم؟
              </h2>
              <p className="mt-3 text-[13px] leading-7 text-[#94A3B8] sm:text-[13.5px]">
                مسیر مناسب خود را انتخاب کنید و گام به گام پیش بروید. با تکمیل هر مرحله، مهارت‌های خود را بسنجید و به مرحله بعدی بروید.
              </p>
              <div className="mt-5 flex justify-center md:justify-start">
                <Link
                  href="/articles"
                  className="focus-ring inline-flex h-11 items-center justify-center gap-2 rounded-xl border border-white/[0.1] bg-[#0F1626] px-5 text-[13px] font-bold text-[#E5E7EB] no-underline transition hover:border-[rgba(168,85,247,0.4)] hover:text-white"
                >
                  <BookIcon className="h-4 w-4 shrink-0 text-[#A78BFA]" />
                  راهنمای کامل
                </Link>
              </div>
            </div>
          </div>
        </div>
      </RoadmapContainer>
    </section>
  );
}
