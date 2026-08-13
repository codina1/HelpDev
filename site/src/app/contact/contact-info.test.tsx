import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { CONTACT_GITHUB, ContactInfo } from "./contact-info";

describe("contact information", () => {
  it("renders email, social, and support cards without invented channels", () => {
    const html = renderToStaticMarkup(<ContactInfo />);
    expect(html).toContain("ایمیل");
    expect(html).toContain("شبکه‌های اجتماعی");
    expect(html).toContain("پشتیبانی");
    expect(html).toContain("تلگرام");
    expect(html).toContain("لینکدین");
    expect(html).toContain("GitHub");
    expect(html).toContain(CONTACT_GITHUB);
    expect(html).not.toContain("mailto:");
    expect(html).not.toContain("https://t.me");
    expect(html).not.toContain("linkedin.com");
  });
});
