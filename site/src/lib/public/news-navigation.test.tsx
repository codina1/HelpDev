import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { FeaturedNews } from "@/components/news/featured-news";
import { NewsArticleCard } from "@/components/news/news-article-card";
import { PopularNewsSidebar } from "@/components/news/popular-news-sidebar";
import { NEWS_ARTICLES, getNewsArticleBySlug } from "@/data/news-articles";
import { publicHrefForContent } from "@/lib/public/content-helpers";
import { publicNewsPath } from "@/lib/public/map-published-news";
import { hrefForSearchResult } from "@/lib/public/search-navigation";

describe("news detail navigation", () => {
  it("maps news hrefs to /news/{slug}", () => {
    expect(publicNewsPath("cursor-1-ai-ide")).toBe("/news/cursor-1-ai-ide");
    expect(publicHrefForContent({ type: "News", slug: "mcp-standard-tools" })).toBe(
      "/news/mcp-standard-tools",
    );
    expect(
      hrefForSearchResult({
        sourceType: "news",
        sourceId: "1",
        title: "N",
        slug: "claude-terminal-agent",
      }),
    ).toBe("/news/claude-terminal-agent");
  });

  it("keeps catalog slug lookup for refresh-safe detail fallback", () => {
    for (const item of NEWS_ARTICLES) {
      expect(getNewsArticleBySlug(item.slug)?.slug).toBe(item.slug);
    }
    expect(getNewsArticleBySlug("missing-news")).toBeNull();
  });

  it("makes featured, grid, and popular items link to matching news detail", () => {
    const featured = NEWS_ARTICLES[0]!;
    const featuredHtml = renderToStaticMarkup(<FeaturedNews article={featured} />);
    expect(featuredHtml).toContain(`/news/${featured.slug}`);
    expect(featuredHtml).toContain("افزودن به ذخیره‌ها");

    const card = NEWS_ARTICLES[1]!;
    const cardHtml = renderToStaticMarkup(<NewsArticleCard article={card} />);
    expect(cardHtml).toContain(`/news/${card.slug}`);
    expect(cardHtml).toContain(card.title);

    const popularHtml = renderToStaticMarkup(<PopularNewsSidebar />);
    for (const item of NEWS_ARTICLES.slice(0, 5)) {
      expect(popularHtml).toContain(`/news/${item.slug}`);
    }
    expect(popularHtml).not.toContain("#news-");
  });
});
