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
          <h2 id="home-newsletter-heading" className="home-newsletter-heading">
            از تازه‌های مهندسی باخبر شوید
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
