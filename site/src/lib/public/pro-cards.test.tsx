import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import {
  AiFeatureCard,
  ArticleCardPro,
  RoadmapCardPro,
  ToolCardPro,
} from "@/components/public/pro";
import { inferTechTags, softUseCases } from "@/lib/public/display-meta";

describe("Sprint 50D-2 — Pro cards", () => {
  it("renders ArticleCardPro with cover metadata", () => {
    const html = renderToStaticMarkup(
      <ArticleCardPro
        title="ASP.NET Microservices"
        href="/articles/aspnet"
        category="مقاله"
        readingTime="۸ دقیقه"
        difficulty="متوسط"
        tags={[".NET"]}
        featured
      />,
    );
    expect(html).toContain("ASP.NET Microservices");
    expect(html).toContain(".NET");
    expect(html).toContain("۸ دقیقه");
  });

  it("renders ToolCardPro with rating placeholder chrome, not fake scores", () => {
    const html = renderToStaticMarkup(
      <ToolCardPro
        title="Cursor"
        href="/toolbox"
        category="AI"
        useCases={["بهره‌وری AI"]}
        stackTags={["AI"]}
      />,
    );
    expect(html).toContain("Cursor");
    expect(html).toContain("امتیاز به‌زودی");
    expect(html).not.toContain("4.8");
  });

  it("renders RoadmapCardPro progress track and AiFeatureCard", () => {
    expect(
      renderToStaticMarkup(
        <RoadmapCardPro
          title="Frontend"
          href="/roadmap"
          level="میانی"
          nodes={[{ label: "React" }, { label: "Next.js" }]}
        />,
      ),
    ).toContain("پیش‌نمای ساختاری");

    expect(
      renderToStaticMarkup(
        <AiFeatureCard title="دستیار" description="توضیح" />,
      ),
    ).toContain("Engineering Intelligence");
  });

  it("infers tech tags only from title/slug text", () => {
    expect(inferTechTags("Learning React with Next.js", "react-next")).toEqual(
      expect.arrayContaining(["React", "Next.js"]),
    );
    expect(inferTechTags("Hello world", "hello")).toEqual([]);
    expect(softUseCases("ai-tools")).toContain("بهره‌وری AI");
  });
});
