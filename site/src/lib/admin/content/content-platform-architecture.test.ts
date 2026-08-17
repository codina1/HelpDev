import { describe, expect, it } from "vitest";
import { existsSync } from "node:fs";
import { join } from "node:path";
import {
  CONTENT_WORKSPACE_IDS,
  ContentWorkspaceRegistry,
  getContentWorkspace,
  listContentWorkspaces,
} from "@/lib/admin/content/registry";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { ADMIN_NAVIGATION, filterAdminNavigation, flattenNavItems } from "@/lib/admin/navigation";
import {
  WORKSPACE_EDITORS,
  WORKSPACE_LISTS,
  resolveWorkspaceEditor,
  resolveWorkspaceList,
} from "@/components/admin/content/workspaces/workspace-editors";

describe("Sprint 47A — ContentWorkspaceRegistry", () => {
  it("registers all platform workspaces", () => {
    expect(CONTENT_WORKSPACE_IDS).toEqual([
      "article",
      "news",
      "tool",
      "roadmap",
      "prompt",
      "comparison",
      "tutorial",
    ]);
    expect(listContentWorkspaces()).toHaveLength(7);
  });

  it("maps persisted workspaces to content-api with locked types", () => {
    expect(getContentWorkspace("article").persistence).toBe("content-api");
    expect(getContentWorkspace("article").contentType).toBe("Article");
    expect(getContentWorkspace("news").contentType).toBe("News");
    expect(getContentWorkspace("tool").persistence).toBe("content-api");
    expect(getContentWorkspace("tool").contentType).toBe("Tool");
    expect(getContentWorkspace("roadmap").persistence).toBe("content-api");
    expect(getContentWorkspace("roadmap").contentType).toBe("Roadmap");
    expect(getContentWorkspace("tutorial").persistence).toBe("content-api");
    expect(getContentWorkspace("tutorial").contentType).toBe("Course");
  });

  it("keeps comparison without persistence", () => {
    expect(ContentWorkspaceRegistry.comparison.persistence).toBe("none");
    expect(ContentWorkspaceRegistry.comparison.contentType).toBeUndefined();
  });

  it("delegates prompts to Prompt Lab", () => {
    expect(getContentWorkspace("prompt").persistence).toBe("prompt-lab");
    expect(getContentWorkspace("prompt").route).toBe(ADMIN_ROUTES.contentPrompts);
  });

  it("wires editor and list components for every id", () => {
    for (const id of CONTENT_WORKSPACE_IDS) {
      expect(resolveWorkspaceEditor(id)).toBe(WORKSPACE_EDITORS[id]);
      expect(resolveWorkspaceList(id)).toBe(WORKSPACE_LISTS[id]);
    }
  });
});

describe("Sprint 47A — routes", () => {
  const appRoot = join(process.cwd(), "src", "app");
  const pageExists = (...segments: string[]) =>
    existsSync(join(appRoot, ...segments, "page.tsx"));

  it("exposes hub, all-list, and every workspace route", () => {
    expect(pageExists("admin", "content")).toBe(true);
    expect(pageExists("admin", "content", "all")).toBe(true);
    for (const slug of [
      "articles",
      "news",
      "tools",
      "roadmaps",
      "prompts",
      "comparisons",
      "tutorials",
    ]) {
      expect(pageExists("admin", "content", slug), slug).toBe(true);
      expect(pageExists("admin", "content", slug, "new"), `${slug}/new`).toBe(true);
    }
    expect(pageExists("admin", "content", "tutorials", "[id]")).toBe(true);
  });

  it("aligns ADMIN_ROUTES with pages", () => {
    const pairs: Array<[string, string[]]> = [
      [ADMIN_ROUTES.content, ["admin", "content"]],
      [ADMIN_ROUTES.contentAll, ["admin", "content", "all"]],
      [ADMIN_ROUTES.contentComparisons, ["admin", "content", "comparisons"]],
      [ADMIN_ROUTES.contentComparisonsNew, ["admin", "content", "comparisons", "new"]],
      [ADMIN_ROUTES.contentTutorials, ["admin", "content", "tutorials"]],
      [ADMIN_ROUTES.contentTutorialsNew, ["admin", "content", "tutorials", "new"]],
    ];
    for (const [route, segments] of pairs) {
      expect(pageExists(...segments), route).toBe(true);
    }
  });
});

describe("Sprint 47A — navigation & permissions", () => {
  it("lists specialized content items for Admin", () => {
    const titles = ADMIN_NAVIGATION.find((g) => g.id === "content")!.items.map((i) => i.title);
    expect(titles).toEqual(
      expect.arrayContaining([
        "مقالات",
        "اخبار",
        "ابزارها",
        "نقشه راه",
        "Prompt Lab",
        "مقایسه‌ها",
        "آموزش‌ها",
        "پلتفرم محتوا",
      ]),
    );
  });

  it("preserves permission gating", () => {
    expect(filterAdminNavigation(ADMIN_NAVIGATION, "User")).toHaveLength(0);
    expect(filterAdminNavigation(ADMIN_NAVIGATION, "Writer")).toHaveLength(0);
    const admin = filterAdminNavigation(ADMIN_NAVIGATION, "Admin");
    expect(admin.some((g) => g.id === "content")).toBe(true);
  });

  it("keeps learning separate", () => {
    expect(ADMIN_NAVIGATION.find((g) => g.id === "learning")?.title).toBe("آموزش");
  });

  it("uses content.view for workspace nav items", () => {
    const items = flattenNavItems(ADMIN_NAVIGATION).filter((i) =>
      [
        "content-articles",
        "content-news",
        "content-tools",
        "content-roadmaps",
        "content-prompts",
        "content-comparisons",
        "content-tutorials",
      ].includes(i.id),
    );
    expect(items).toHaveLength(7);
    for (const item of items) {
      expect(item.permission).toBe("content.view");
      expect(item.href?.startsWith("/admin/content/")).toBe(true);
    }
  });
});
