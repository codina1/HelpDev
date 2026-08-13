import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { HOME_TRUST_MARKS, HomeTrustSection } from "@/components/public/home/home-trust-section";

describe("homepage trust strip", () => {
  it("renders the glass heading and local monochrome placeholders", () => {
    const html = renderToStaticMarkup(<HomeTrustSection />);
    expect(html).toContain("مورد اعتماد تیم‌های حرفه‌ای");
    expect(html).toContain("home-trust-panel");
    expect(html).toContain("home-trust-row");
    expect(html).toContain("/next.svg");
    expect(html).toContain("/vercel.svg");
    for (const mark of HOME_TRUST_MARKS) {
      expect(html).toContain(mark.name);
    }
    expect(html).not.toContain("http://");
    expect(html).not.toContain("https://");
  });
});
