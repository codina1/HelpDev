import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { AICommandBox } from "@/components/ui/public/v2";
import { hrefForSearchResult, labelForSearchSource } from "@/lib/public/search-navigation";

describe("Sprint 50B — search UI", () => {
  it("AICommandBox exposes Ctrl+K affordance and suggestions", () => {
    const html = renderToStaticMarkup(<AICommandBox onOpenPalette={() => undefined} />);
    expect(html).toContain("Ctrl");
    expect(html).toContain("K");
    expect(html).toContain("Microservice");
  });

  it("maps search results for articles tools roadmaps courses", () => {
    expect(
      hrefForSearchResult({
        sourceType: "content",
        sourceId: "1",
        title: "A",
        slug: "a",
      }),
    ).toBe("/articles/a");
    expect(
      hrefForSearchResult({
        sourceType: "tool",
        sourceId: "2",
        title: "T",
        slug: "t",
      }),
    ).toBe("/tools/t");
    expect(
      hrefForSearchResult({
        sourceType: "roadmap",
        sourceId: "3",
        title: "R",
        slug: "r",
      }),
    ).toContain("/roadmap");
    expect(
      hrefForSearchResult({
        sourceType: "course",
        sourceId: "4",
        title: "C",
        slug: "c",
      }),
    ).toContain("/courses");
    expect(
      labelForSearchSource({
        sourceType: "tool",
        sourceId: "2",
        title: "T",
        slug: "t",
      }),
    ).toBe("ابزار");
  });
});
