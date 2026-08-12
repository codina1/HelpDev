import { describe, expect, it } from "vitest";
import { existsSync } from "node:fs";
import { join } from "node:path";
import {
  CONTENT_TYPE_REGISTRY,
  CONTENT_WORKSPACE_KEYS,
  getWorkspaceByContentType,
  getWorkspaceByKey,
  resolveEditorKey,
} from "@/lib/admin/content/factory";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { ADMIN_NAVIGATION, filterAdminNavigation, flattenNavItems } from "@/lib/admin/navigation";
import { WORKSPACE_EDITORS, resolveWorkspaceEditor } from "@/components/admin/content/workspaces/workspace-editors";

describe("Sprint 46.6 / 47A — content workspace factory (compat)", () => {
  it("maps API-backed workspaces to ContentType; foundations stay none", () => {
    expect(CONTENT_TYPE_REGISTRY.article.contentType).toBe("Article");
    expect(CONTENT_TYPE_REGISTRY.news.contentType).toBe("News");
    expect(CONTENT_TYPE_REGISTRY.tool.contentType).toBe("Tool");
    expect(CONTENT_TYPE_REGISTRY.roadmap.contentType).toBe("Roadmap");
    expect(CONTENT_TYPE_REGISTRY.prompt.contentType).toBe("none");
    expect(CONTENT_TYPE_REGISTRY.comparison.contentType).toBe("none");
    expect(CONTENT_TYPE_REGISTRY.tutorial.contentType).toBe("none");
  });

  it("resolves workspace by content type for API-backed types only", () => {
    expect(getWorkspaceByContentType("Article")?.key).toBe("article");
    expect(getWorkspaceByContentType("News")?.key).toBe("news");
    expect(getWorkspaceByContentType("Tool")?.key).toBe("tool");
    expect(getWorkspaceByContentType("Roadmap")?.key).toBe("roadmap");
    expect(getWorkspaceByContentType("RoadmapStep")).toBeUndefined();
  });

  it("maps editors for all workspace keys without duplication of keys", () => {
    for (const key of CONTENT_WORKSPACE_KEYS) {
      expect(resolveEditorKey(key)).toBe(key);
      expect(WORKSPACE_EDITORS[key]).toBeDefined();
      expect(resolveWorkspaceEditor(key)).toBe(WORKSPACE_EDITORS[key]);
    }
  });

  it("delegates prompt workspace to Prompt Lab", () => {
    expect(getWorkspaceByKey("prompt").delegatesToPromptLab).toBe(true);
    expect(getWorkspaceByKey("prompt").listHref).toBe(ADMIN_ROUTES.contentPrompts);
  });
});

describe("Sprint 46.6 — workspace routes", () => {
  const appRoot = join(process.cwd(), "src", "app");

  function pageExists(...segments: string[]): boolean {
    return existsSync(join(appRoot, ...segments, "page.tsx"));
  }

  it("exposes list and new pages for all workspaces", () => {
    expect(pageExists("admin", "content", "articles")).toBe(true);
    expect(pageExists("admin", "content", "articles", "new")).toBe(true);
    expect(pageExists("admin", "content", "articles", "[id]")).toBe(true);
    expect(pageExists("admin", "content", "news")).toBe(true);
    expect(pageExists("admin", "content", "news", "new")).toBe(true);
    expect(pageExists("admin", "content", "tools")).toBe(true);
    expect(pageExists("admin", "content", "tools", "new")).toBe(true);
    expect(pageExists("admin", "content", "tools", "[id]")).toBe(true);
    expect(pageExists("admin", "content", "roadmaps")).toBe(true);
    expect(pageExists("admin", "content", "roadmaps", "new")).toBe(true);
    expect(pageExists("admin", "content", "roadmaps", "[id]")).toBe(true);
    expect(pageExists("admin", "content", "prompts")).toBe(true);
    expect(pageExists("admin", "content", "prompts", "new")).toBe(true);
  });

  it("keeps legacy content routes working", () => {
    expect(pageExists("admin", "content")).toBe(true);
    expect(pageExists("admin", "content", "new")).toBe(true);
    expect(pageExists("admin", "content", "[id]")).toBe(true);
    expect(pageExists("admin", "content", "[id]", "edit")).toBe(true);
  });

  it("aligns ADMIN_ROUTES workspace entries with pages", () => {
    const routes: Array<[string, string[]]> = [
      [ADMIN_ROUTES.contentArticles, ["admin", "content", "articles"]],
      [ADMIN_ROUTES.contentArticlesNew, ["admin", "content", "articles", "new"]],
      [ADMIN_ROUTES.contentNews, ["admin", "content", "news"]],
      [ADMIN_ROUTES.contentNewsNew, ["admin", "content", "news", "new"]],
      [ADMIN_ROUTES.contentTools, ["admin", "content", "tools"]],
      [ADMIN_ROUTES.contentToolsNew, ["admin", "content", "tools", "new"]],
      [ADMIN_ROUTES.contentRoadmaps, ["admin", "content", "roadmaps"]],
      [ADMIN_ROUTES.contentRoadmapsNew, ["admin", "content", "roadmaps", "new"]],
      [ADMIN_ROUTES.contentPrompts, ["admin", "content", "prompts"]],
      [ADMIN_ROUTES.contentPromptsNew, ["admin", "content", "prompts", "new"]],
    ];
    for (const [route, segments] of routes) {
      expect(pageExists(...segments), `missing ${route}`).toBe(true);
    }
  });
});

describe("Sprint 46.6 — navigation & permissions", () => {
  it("lists workspace items under محتوا for Admin", () => {
    const contentGroup = ADMIN_NAVIGATION.find((g) => g.id === "content");
    expect(contentGroup).toBeDefined();
    const titles = contentGroup!.items.map((i) => i.title);
    expect(titles).toEqual(
      expect.arrayContaining(["مقالات", "اخبار", "ابزارها", "نقشه راه", "Prompt Lab"]),
    );
  });

  it("keeps learning separate from content workspaces", () => {
    const learning = ADMIN_NAVIGATION.find((g) => g.id === "learning");
    expect(learning?.title).toBe("آموزش");
    expect(learning?.items.some((i) => i.href === ADMIN_ROUTES.learning)).toBe(true);
  });

  it("preserves permission filtering (non-admin sees no nav)", () => {
    expect(filterAdminNavigation(ADMIN_NAVIGATION, "User")).toHaveLength(0);
    expect(filterAdminNavigation(ADMIN_NAVIGATION, "Writer")).toHaveLength(0);
  });

  it("gives Admin content workspace hrefs that stay under /admin/content", () => {
    const filtered = filterAdminNavigation(ADMIN_NAVIGATION, "Admin");
    const contentItems = flattenNavItems(filtered).filter((i) =>
      i.id.startsWith("content-articles") ||
      i.id.startsWith("content-news") ||
      i.id.startsWith("content-tools") ||
      i.id.startsWith("content-roadmaps") ||
      i.id.startsWith("content-prompts"),
    );
    expect(contentItems.length).toBeGreaterThanOrEqual(5);
    for (const item of contentItems) {
      expect(item.href?.startsWith("/admin/content/")).toBe(true);
      expect(item.permission).toBe("content.view");
    }
  });
});
