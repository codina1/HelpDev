import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { HOME_TRUST_MARKS, HomeTrustSection } from "@/components/public/home/home-trust-section";

describe("homepage trust strip", () => {
  it("renders the glass heading and local stack tiles, not a customer roster", () => {
    const html = renderToStaticMarkup(<HomeTrustSection />);
    expect(html).toContain("مورد اعتماد تیم‌های حرفه‌ای");
    expect(html).toContain("نه فهرست مشتری");
    expect(html).toContain("home-trust-panel");
    expect(html).toContain("home-trust-row");
    expect(html).toContain("home-trust-icon");
    expect(html).toContain("home-section-title");
    expect(html).toContain("text-start");
    for (const mark of HOME_TRUST_MARKS) {
      expect(html).toContain(mark.name);
      expect(html).toContain(`home-trust-mark-${mark.accent}`);
    }
    expect(html).not.toContain("http://");
    expect(html).not.toContain("https://");
    expect(html).not.toContain("/next.svg");
    expect(html).not.toContain("/vercel.svg");
  });
});
