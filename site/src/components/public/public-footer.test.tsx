import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { PublicFooter } from "@/components/public/public-footer";

describe("public footer", () => {
  it("renders brand, columns, CTA, social, copyright, and legal links", () => {
    const html = renderToStaticMarkup(<PublicFooter />);
    expect(html).toContain("pub-footer");
    expect(html).toContain("HelpDev");
    expect(html).toContain("محصول");
    expect(html).toContain("یادگیری");
    expect(html).toContain("ابزارها");
    expect(html).toContain("شرکت");
    expect(html).toContain("ورود به پلتفرم");
    expect(html).not.toContain("خبرنامه HelpDev");
    expect(html).not.toContain("عضویت");
    expect(html).toContain("https://github.com/codina1/HelpDev");
    expect(html).toContain("/about");
    expect(html).toContain("/contact");
    expect(html).toContain("/privacy");
    expect(html).toContain("/terms");
    expect(html).toContain("تمامی حقوق محفوظ است");
    expect(html).not.toContain("https://twitter.com");
    expect(html).not.toContain("https://t.me");
  });
});
