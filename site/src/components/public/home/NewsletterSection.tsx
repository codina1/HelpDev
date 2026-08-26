import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { HomeNewsletterForm } from "@/components/public/home/home-newsletter-form";

/** Replace `/public/home/icon-newsletter.png` to update the banner icon. */
export const NEWSLETTER_ICON_SRC = "/home/icon-newsletter.png";

/**
 * Newsletter Banner — Design Reference purple glass CTA above footer.
 */
export function NewsletterSection() {
  return (
    <section
      className="home-newsletter-banner relative bg-[#050816] py-8 sm:py-10 lg:py-12"
      aria-labelledby="newsletter-heading"
    >
      <PublicContainer size="wide">
        <div className="relative overflow-hidden rounded-[18px] border border-white/[0.08] bg-[#0B1224] px-5 py-7 shadow-[0_0_40px_rgba(124,58,237,0.18)] sm:px-8 sm:py-8 lg:px-10">
          <div
            className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_70%_80%_at_100%_0%,rgba(124,58,237,0.28),transparent_55%)]"
            aria-hidden
          />
          <div
            className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_50%_60%_at_0%_100%,rgba(37,99,235,0.16),transparent_50%)]"
            aria-hidden
          />

          <div className="relative flex flex-col items-stretch gap-6 lg:flex-row lg:items-center lg:justify-between lg:gap-10">
            <div className="flex min-w-0 flex-1 items-start gap-4 text-start sm:items-center">
              <span
                className="flex h-14 w-14 shrink-0 items-center justify-center drop-shadow-[0_0_20px_rgba(168,85,247,0.45)]"
                aria-hidden
              >
                {/* Icon slot — swap NEWSLETTER_ICON_SRC asset */}
                <img
                  src={NEWSLETTER_ICON_SRC}
                  alt=""
                  width={56}
                  height={56}
                  decoding="async"
                  className="h-14 w-14 object-contain"
                  data-icon-slot="newsletter"
                />
              </span>
              <div className="min-w-0">
                <h2 id="newsletter-heading" className="text-[1.25rem] font-extrabold text-white sm:text-[1.45rem]">
                  از تازه‌های HelpDev باخبر شوید
                </h2>
                <p className="mt-2 text-[13px] leading-7 text-[#94A3B8] sm:text-[14px]">
                  خلاصه مقالات، مسیرها و ابزارهای جدید را با ایمیل دریافت کنید.
                </p>
              </div>
            </div>

            <div className="w-full shrink-0 lg:max-w-md">
              <HomeNewsletterForm />
            </div>
          </div>
        </div>
      </PublicContainer>
    </section>
  );
}
