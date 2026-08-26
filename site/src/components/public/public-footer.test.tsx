import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { Footer, FOOTER_ICON_SLOTS } from "@/components/public/home/Footer";
import { PublicFooter } from "@/components/public/public-footer";

describe("public footer", () => {
  it("renders brand, columns, social slots, copyright, and legal links", () => {
    const html = renderToStaticMarkup(<Footer />);
    expect(html).toContain("home-footer");
    expect(html).toContain("HelpDev");
    expect(html).toContain("محصول");
    expect(html).toContain("منابع");
    expect(html).toContain("شرکت");
    expect(html).toContain("انجمن");
    expect(html).toContain(FOOTER_ICON_SLOTS.brand);
    expect(html).toContain(FOOTER_ICON_SLOTS.github);
    expect(html).toContain('data-icon-slot="brand"');
    expect(html).toContain("https://github.com/codina1/HelpDev");
    expect(html).toContain("/about");
    expect(html).toContain("/contact");
    expect(html).toContain("/privacy");
    expect(html).toContain("/terms");
    expect(html).toContain("تمامی حقوق محفوظ است");
    expect(html).not.toContain("https://twitter.com");
    expect(html).not.toContain("https://t.me");
  });

  it("keeps PublicFooter as a thin alias of Footer", () => {
    const a = renderToStaticMarkup(<Footer />);
    const b = renderToStaticMarkup(<PublicFooter />);
    expect(a).toBe(b);
  });
});
