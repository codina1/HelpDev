import { describe, expect, it } from "vitest";
import { existsSync } from "node:fs";
import { join } from "node:path";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { HEADER_NAV } from "@/lib/constants";

/**
 * Sprint 46 — frontend release certification: critical routes exist; no orphan admin nav targets.
 */
describe("Sprint 46 — frontend release certification", () => {
  const appRoot = join(process.cwd(), "src", "app");

  function pageExists(...segments: string[]): boolean {
    return existsSync(join(appRoot, ...segments, "page.tsx"));
  }

  it("exposes public landing, content, and search pages", () => {
    expect(pageExists()).toBe(true); // src/app/page.tsx
    expect(existsSync(join(appRoot, "page.tsx"))).toBe(true);
    expect(pageExists("articles")).toBe(true);
    expect(pageExists("search")).toBe(true);
    expect(pageExists("courses")).toBe(true);
  });

  it("exposes user dashboard, learning, assistant, and settings", () => {
    expect(pageExists("dashboard")).toBe(true);
    expect(pageExists("learning")).toBe(true);
    expect(pageExists("learning", "assistant")).toBe(true);
    expect(pageExists("settings")).toBe(true);
    expect(pageExists("profile")).toBe(true);
  });

  it("exposes admin dashboard and operational surfaces", () => {
    expect(pageExists("admin")).toBe(true);
    expect(pageExists("admin", "content")).toBe(true);
    expect(pageExists("admin", "seo")).toBe(true);
    expect(pageExists("admin", "media")).toBe(true);
    expect(pageExists("admin", "content", "workflows")).toBe(true);
    expect(pageExists("admin", "ai")).toBe(true);
    expect(pageExists("admin", "analytics")).toBe(true);
    expect(pageExists("admin", "operations")).toBe(true);
  });

  it("keeps ADMIN_ROUTES aligned with existing page files", () => {
    const routeToSegments: Record<string, string[]> = {
      [ADMIN_ROUTES.dashboard]: ["admin"],
      [ADMIN_ROUTES.content]: ["admin", "content"],
      [ADMIN_ROUTES.contentNew]: ["admin", "content", "new"],
      [ADMIN_ROUTES.contentWorkflows]: ["admin", "content", "workflows"],
      [ADMIN_ROUTES.contentArticles]: ["admin", "content", "articles"],
      [ADMIN_ROUTES.contentArticlesNew]: ["admin", "content", "articles", "new"],
      [ADMIN_ROUTES.contentNews]: ["admin", "content", "news"],
      [ADMIN_ROUTES.contentNewsNew]: ["admin", "content", "news", "new"],
      [ADMIN_ROUTES.contentTools]: ["admin", "content", "tools"],
      [ADMIN_ROUTES.contentToolsNew]: ["admin", "content", "tools", "new"],
      [ADMIN_ROUTES.contentRoadmaps]: ["admin", "content", "roadmaps"],
      [ADMIN_ROUTES.contentRoadmapsNew]: ["admin", "content", "roadmaps", "new"],
      [ADMIN_ROUTES.contentPrompts]: ["admin", "content", "prompts"],
      [ADMIN_ROUTES.contentPromptsNew]: ["admin", "content", "prompts", "new"],
      [ADMIN_ROUTES.contentComparisons]: ["admin", "content", "comparisons"],
      [ADMIN_ROUTES.contentComparisonsNew]: ["admin", "content", "comparisons", "new"],
      [ADMIN_ROUTES.contentTutorials]: ["admin", "content", "tutorials"],
      [ADMIN_ROUTES.contentTutorialsNew]: ["admin", "content", "tutorials", "new"],
      [ADMIN_ROUTES.contentAll]: ["admin", "content", "all"],
      [ADMIN_ROUTES.media]: ["admin", "media"],
      [ADMIN_ROUTES.seo]: ["admin", "seo"],
      [ADMIN_ROUTES.learning]: ["admin", "learning"],
      [ADMIN_ROUTES.toolbox]: ["admin", "toolbox"],
      [ADMIN_ROUTES.promptLab]: ["admin", "prompt-lab"],
      [ADMIN_ROUTES.users]: ["admin", "users"],
      [ADMIN_ROUTES.analytics]: ["admin", "analytics"],
      [ADMIN_ROUTES.analyticsContent]: ["admin", "analytics", "content"],
      [ADMIN_ROUTES.ai]: ["admin", "ai"],
      [ADMIN_ROUTES.searchKnowledge]: ["admin", "search", "knowledge"],
      [ADMIN_ROUTES.audit]: ["admin", "audit"],
      [ADMIN_ROUTES.operations]: ["admin", "operations"],
      [ADMIN_ROUTES.settings]: ["admin", "settings"],
    };

    for (const [route, segments] of Object.entries(routeToSegments)) {
      expect(pageExists(...segments), `missing page for ${route}`).toBe(true);
    }
  });

  it("keeps header nav targets on existing routes", () => {
    for (const item of HEADER_NAV) {
      if (item.href === "/") {
        expect(existsSync(join(appRoot, "page.tsx"))).toBe(true);
        continue;
      }
      const segments = item.href.replace(/^\//, "").split("/");
      expect(pageExists(...segments), `missing page for header nav ${item.href}`).toBe(true);
    }
  });
});
