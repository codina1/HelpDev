import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import {
  LatestArticlesSection,
  buildLatestArticles,
  categoryForLatestArticle,
} from "@/components/public/home/LatestArticlesSection";
import { HomeArticlesSection } from "@/components/public/home/home-articles-section";
import type { ContentSummaryDto } from "@/lib/api/content";

const published: ContentSummaryDto = {
  id: "a1",
  title: "ASP.NET Core Outbox Pattern",
  slug: "aspnet-outbox",
  type: "Article",
  status: "Published",
  views: 0,
  saves: 0,
  createdAt: "2026-08-01T10:00:00.000Z",
  coverImage: "/home/icon-dotnet.png",
};

describe("homepage latest articles", () => {
  it("renders reference header and empty state without mock cards", () => {
    const html = renderToStaticMarkup(<LatestArticlesSection articles={[]} />);
    expect(html).toContain("جدیدترین مقالات");
    expect(html).toContain("تازه‌ترین آموزش‌ها و تحلیل‌های دنیای توسعه، هوش مصنوعی و مهندسی نرم‌افزار");
    expect(html).toContain("مشاهده همه مقالات");
    expect(html).toContain("/articles");
    expect(html).toContain("هنوز مقاله‌ای منتشر نشده است");
    expect(html).not.toContain("رشد Modular Monolith");
    expect(buildLatestArticles([])).toHaveLength(0);
  });

  it("renders API articles in responsive glass grid", () => {
    const html = renderToStaticMarkup(<LatestArticlesSection articles={[published]} />);
    expect(html).toContain(published.title);
    expect(html).toContain("/articles/aspnet-outbox");
    expect(html).toContain("/home/icon-dotnet.png");
    expect(html).toContain("rounded-[18px]");
    expect(html).toContain("bg-[#0B1224]");
    expect(html).toContain("hover:-translate-y-[6px]");
    expect(html).toContain("grid-cols-1");
    expect(html).toContain("sm:grid-cols-3");
    expect(html).toContain("lg:grid-cols-5");
    expect(html).toContain("دقیقه مطالعه");
    expect(buildLatestArticles([published])).toHaveLength(1);
    expect(buildLatestArticles([published])[0]?.date).not.toBe("");
  });

  it("keeps HomeArticlesSection as a thin alias", () => {
    const a = renderToStaticMarkup(<LatestArticlesSection articles={[published]} />);
    const b = renderToStaticMarkup(<HomeArticlesSection articles={[published]} />);
    expect(a).toBe(b);
  });

  it("classifies HelpDev technical titles into categories", () => {
    expect(categoryForLatestArticle("RAG retrieval", "rag-guide")).toBe("هوش مصنوعی");
    expect(categoryForLatestArticle("MCP protocol", "mcp")).toBe("MCP");
    expect(categoryForLatestArticle("Modular monolith", "modular")).toBe("معماری");
  });
});
