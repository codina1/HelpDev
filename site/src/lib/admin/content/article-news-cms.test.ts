import { describe, expect, it } from "vitest";
import { existsSync, readFileSync } from "node:fs";
import { join } from "node:path";
import {
  buildArticlePayload,
  buildNewsPayload,
  validateArticleForm,
  validateNewsForm,
} from "@/lib/admin/content/content-mappers";
import { CONTENT_CAPABILITIES } from "@/lib/admin/content/content-api";
import { EMPTY_ARTICLE_FORM, EMPTY_NEWS_FORM } from "@/lib/admin/content/content-types";
import { ADMIN_ROUTES } from "@/lib/admin/routes";

describe("Sprint 47B — article/news CMS frontend", () => {
  it("exposes article and news metadata capabilities", () => {
    expect(CONTENT_CAPABILITIES.articleMetadata).toBe(true);
    expect(CONTENT_CAPABILITIES.newsMetadata).toBe(true);
  });

  it("validates article reading time and news source", () => {
    expect(validateArticleForm({ ...EMPTY_ARTICLE_FORM, readingTimeMinutes: "0" }).readingTimeMinutes).toBeTruthy();
    expect(validateNewsForm({ ...EMPTY_NEWS_FORM, sourceName: "" }).sourceName).toBeTruthy();
    expect(
      validateNewsForm({
        ...EMPTY_NEWS_FORM,
        sourceName: "Wire",
        sourceUrl: "https://helpdev.example",
        newsDateUtc: "2026-07-23T10:00",
      }),
    ).toEqual({});
  });

  it("builds article/news payloads without inventing fields", () => {
    expect(buildArticlePayload({ ...EMPTY_ARTICLE_FORM, readingTimeMinutes: "7", isFeatured: true })).toEqual({
      categoryId: null,
      difficultyLevel: "Beginner",
      readingTimeMinutes: 7,
      isFeatured: true,
      allowComments: true,
      tableOfContentsEnabled: true,
    });

    const news = buildNewsPayload({
      ...EMPTY_NEWS_FORM,
      sourceName: "Wire",
      sourceUrl: "https://helpdev.example/s",
      priority: "Breaking",
      newsDateUtc: "2026-07-23T10:00",
    });
    expect(news.sourceName).toBe("Wire");
    expect(news.priority).toBe("Breaking");
    expect(news.sourceUrl).toBe("https://helpdev.example/s");
  });

  it("keeps article and news detail routes", () => {
    const app = join(process.cwd(), "src", "app");
    expect(existsSync(join(app, "admin", "content", "articles", "[id]", "page.tsx"))).toBe(true);
    expect(existsSync(join(app, "admin", "content", "news", "[id]", "page.tsx"))).toBe(true);
    expect(existsSync(join(app, "admin", "content", "news", "new", "page.tsx"))).toBe(true);
    expect(ADMIN_ROUTES.contentArticles).toBe("/admin/content/articles");
    expect(ADMIN_ROUTES.contentNews).toBe("/admin/content/news");
  });

  it("wires settings panels and API paths", () => {
    const api = readFileSync(join(process.cwd(), "src/lib/api/content.ts"), "utf8");
    expect(api).toContain("`/admin/content/${encodeURIComponent(id)}/article`");
    expect(api).toContain("`/admin/content/${encodeURIComponent(id)}/news`");
    expect(
      existsSync(
        join(process.cwd(), "src/components/admin/content/workspaces/article/article-settings-panel.tsx"),
      ),
    ).toBe(true);
    expect(
      existsSync(
        join(process.cwd(), "src/components/admin/content/workspaces/news/news-settings-fields.tsx"),
      ),
    ).toBe(true);
  });
});
