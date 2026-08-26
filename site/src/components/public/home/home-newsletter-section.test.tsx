import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { NEWSLETTER_ICON_SRC, NewsletterSection } from "@/components/public/home/NewsletterSection";
import { HomeNewsletterSection } from "@/components/public/home/home-newsletter-section";
import { isNewsletterEmail } from "@/components/public/home/home-newsletter-form";

describe("homepage newsletter", () => {
  it("renders banner copy, icon slot, email field, and subscribe CTA", () => {
    const html = renderToStaticMarkup(<NewsletterSection />);
    expect(html).toContain("از تازه‌های HelpDev باخبر شوید");
    expect(html).toContain(NEWSLETTER_ICON_SRC);
    expect(html).toContain('data-icon-slot="newsletter"');
    expect(html).toContain("rounded-[18px]");
    expect(html).toContain("bg-[#0B1224]");
    expect(html).toContain('type="email"');
    expect(html).toContain("عضویت");
  });

  it("keeps HomeNewsletterSection as a thin alias", () => {
    const a = renderToStaticMarkup(<NewsletterSection />);
    const b = renderToStaticMarkup(<HomeNewsletterSection />);
    expect(a).toBe(b);
  });

  it("accepts only a well-formed email", () => {
    expect(isNewsletterEmail("dev@helpdev.local")).toBe(true);
    expect(isNewsletterEmail("not-an-email")).toBe(false);
  });
});
