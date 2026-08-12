import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import {
  Badge,
  Container,
  ContentCard,
  GradientHeading,
  RoadmapCard,
  SearchBox,
  Section,
  ToolCard,
} from "@/components/ui/public";
import { extractTocFromBody, publicHrefForContent } from "@/lib/public/content-helpers";
import { hrefForSearchResult, labelForSearchSource } from "@/lib/public/search-navigation";

describe("Sprint 50 — public design system", () => {
  it("renders Container / Section / GradientHeading / Badge", () => {
    const html = renderToStaticMarkup(
      <Section aria-label="demo">
        <Container>
          <GradientHeading as="h2" id="t" subtitle="زیرعنوان">
            عنوان
          </GradientHeading>
          <Badge variant="ai">AI</Badge>
        </Container>
      </Section>,
    );
    expect(html).toContain("عنوان");
    expect(html).toContain("زیرعنوان");
    expect(html).toContain("AI");
    expect(html).toContain('aria-label="demo"');
  });

  it("renders ContentCard / ToolCard / RoadmapCard links", () => {
    expect(
      renderToStaticMarkup(
        <ContentCard title="مقاله یک" href="/articles/a1" typeLabel="مقاله" views={12} />,
      ),
    ).toContain("/articles/a1");

    expect(
      renderToStaticMarkup(<ToolCard title="ابزار" href="/tools/x" category="dev" />),
    ).toContain("/tools/x");

    expect(
      renderToStaticMarkup(
        <RoadmapCard title="مسیر" href="/roadmap" level="مبتدی" stepCount={5} />,
      ),
    ).toContain("مسیر");
  });

  it("renders SearchBox with accessible label", () => {
    const html = renderToStaticMarkup(
      <SearchBox value="" onChange={() => undefined} aria-label="جستجوی تست" />,
    );
    expect(html).toContain('role="search"');
    expect(html).toContain("جستجوی تست");
  });
});

describe("Sprint 50 — public helpers", () => {
  it("maps content hrefs and extracts TOC", () => {
    expect(publicHrefForContent({ type: "Article", slug: "hello" })).toBe("/articles/hello");
    expect(publicHrefForContent({ type: "Tool", slug: "jq" })).toBe("/tools/jq");
    expect(publicHrefForContent({ type: "Roadmap", slug: "fe" })).toContain("/roadmap");

    const toc = extractTocFromBody("## Intro\n\ntext\n### Detail\n");
    expect(toc).toHaveLength(2);
    expect(toc[0].level).toBe(2);
    expect(toc[1].text).toBe("Detail");
  });

  it("maps search results to public routes", () => {
    expect(
      hrefForSearchResult({
        sourceType: "content",
        sourceId: "1",
        title: "Art",
        slug: "art",
      }),
    ).toBe("/articles/art");

    expect(
      hrefForSearchResult({
        sourceType: "tool",
        sourceId: "2",
        title: "T",
        slug: "t1",
      }),
    ).toBe("/tools/t1");

    expect(
      hrefForSearchResult({
        sourceType: "course",
        sourceId: "3",
        title: "C",
        slug: "c1",
      }),
    ).toContain("/courses");

    expect(
      labelForSearchSource({
        sourceType: "roadmap",
        sourceId: "4",
        title: "R",
        slug: "r",
      }),
    ).toBe("نقشه راه");
  });
});
