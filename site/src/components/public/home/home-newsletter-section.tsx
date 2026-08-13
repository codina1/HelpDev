import { PublicSection } from "@/components/ui/public/v2/public-section";
import { HomeNewsletterForm } from "@/components/public/home/home-newsletter-form";

/**
 * Homepage newsletter — glass panel immediately above the site footer.
 */
export function HomeNewsletterSection() {
  return (
    <PublicSection
      className="home-newsletter home-reveal"
      containerSize="wide"
      aria-labelledby="home-newsletter-heading"
    >
      <div className="home-newsletter-panel">
        <div className="home-newsletter-copy">
          <span className="home-newsletter-icon" aria-hidden>
            <NewsletterIcon />
          </span>
          <h2 id="home-newsletter-heading" className="home-newsletter-heading">
            از تازه‌های HelpDev باخبر شوید
          </h2>
          <p className="home-newsletter-lead">
            خلاصه مقالات و مسیرهای منتشرشده HelpDev را با ایمیل دریافت کنید.
          </p>
        </div>
        <HomeNewsletterForm />
      </div>
    </PublicSection>
  );
}

function NewsletterIcon() {
  return (
    <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" aria-hidden>
      <path d="M6 8a6 6 0 1 1 12 0c0 7 3 9 3 9H3s3-2 3-9" />
      <path d="M10 21a2 2 0 0 0 4 0" />
    </svg>
  );
}
