import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import {
  HOME_ARTICLE_TOPICS,
  HomeArticlesSection,
  buildHomeArticles,
  categoryForHomeArticle,
} from "@/components/public/home/home-articles-section";
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
};

describe("homepage latest articles", () => {
  it("renders heading, view-all, and HelpDev topic cards when catalog is empty", () => {
    const html = renderToStaticMarkup(<HomeArticlesSection articles={[]} />);
    expect(html).toContain("تازه‌ترین مقالات");
    expect(html).toContain("/home/cover-");
    expect(html).toContain("همه مقالات");
    expect(html).toContain("/articles");
    expect(html).toContain("home-articles-grid");
    for (const item of HOME_ARTICLE_TOPICS) {
      expect(html).toContain(item.title);
      expect(html).toContain(item.excerpt);
      expect(html).toContain(item.category);
    }
  });

  it("prefers published catalog titles and real dates over topic samples", () => {
    const html = renderToStaticMarkup(<HomeArticlesSection articles={[published]} />);
    expect(html).toContain(published.title);
    expect(html).toContain("/articles/aspnet-outbox");
    expect(html).not.toContain(HOME_ARTICLE_TOPICS[0].title);
    expect(buildHomeArticles([published])).toHaveLength(1);
    expect(buildHomeArticles([published])[0]?.date).not.toBe("");
  });

  it("classifies HelpDev technical titles into Persian categories", () => {
    expect(categoryForHomeArticle("RAG retrieval", "rag-guide")).toBe("هوش مصنوعی");
    expect(categoryForHomeArticle("Modular monolith", "modular")).toBe("معماری");
  });
});
