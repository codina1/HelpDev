import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { CONTACT_FAQ_ITEMS, ContactFaq } from "./contact-faq";

describe("contact FAQ accordion", () => {
  it("renders the three required questions and honest answers", () => {
    const html = renderToStaticMarkup(<ContactFaq />);
    expect(html).toContain("پرسش‌های پرتکرار");
    for (const item of CONTACT_FAQ_ITEMS) {
      expect(html).toContain(item.question);
    }
    expect(html).toContain("/write");
    expect(html).toContain("aria-expanded");
    expect(html).not.toContain("ارسال خودکار");
  });
});
