import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { HomeNewsletterSection } from "@/components/public/home/home-newsletter-section";
import { isNewsletterEmail } from "@/components/public/home/home-newsletter-form";

describe("homepage newsletter", () => {
  it("renders Persian copy, email field, and subscribe CTA", () => {
    const html = renderToStaticMarkup(<HomeNewsletterSection />);
    expect(html).toContain("از تازه‌های HelpDev باخبر شوید");
    expect(html).toContain("home-newsletter-icon");
    expect(html).toContain("خلاصه مقالات و مسیرهای منتشرشده HelpDev");
    expect(html).toContain("home-newsletter-panel");
    expect(html).toContain('type="email"');
    expect(html).toContain("عضویت");
  });

  it("accepts only a well-formed email", () => {
    expect(isNewsletterEmail("dev@helpdev.local")).toBe(true);
    expect(isNewsletterEmail("not-an-email")).toBe(false);
  });
});
