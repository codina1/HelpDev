import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { ABOUT_STAT_LABELS, AboutStats } from "./about-stats";

describe("about page stats", () => {
  it("renders honest catalog counts with the requested labels", () => {
    const html = renderToStaticMarkup(
      <AboutStats counts={{ articles: 8, tools: 5, paths: 3 }} />,
    );
    expect(html).toContain(ABOUT_STAT_LABELS.engineers);
    expect(html).toContain(ABOUT_STAT_LABELS.articles);
    expect(html).toContain(ABOUT_STAT_LABELS.tools);
    expect(html).toContain(ABOUT_STAT_LABELS.paths);
    expect(html).toContain("۰");
    expect(html).toContain("+۸");
    expect(html).toContain("+۵");
    expect(html).toContain("+۳");
    expect(html).not.toContain("45K");
    expect(html).not.toContain("2500");
    expect(html).not.toContain("120");
    expect(html).not.toContain("85");
  });
});
