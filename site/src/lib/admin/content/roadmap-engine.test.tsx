/**
 * @vitest-environment jsdom
 */
import { describe, expect, it } from "vitest";
import { existsSync, readFileSync } from "node:fs";
import { join } from "node:path";
import { renderToStaticMarkup } from "react-dom/server";
import { TopicEditor } from "@/components/admin/content/workspaces/roadmap/topic-editor";
import { ResourcePicker } from "@/components/admin/content/workspaces/roadmap/resource-picker";
import { DragDropOrdering } from "@/components/admin/content/workspaces/roadmap/drag-drop-ordering";
import { CONTENT_CAPABILITIES } from "@/lib/admin/content/content-api";
import { getContentWorkspace } from "@/lib/admin/content/registry";
import { ADMIN_ROUTES } from "@/lib/admin/routes";

describe("Sprint 49 — roadmap engine frontend", () => {
  it("registers roadmap workspace on content-api with Roadmap type", () => {
    expect(CONTENT_CAPABILITIES.roadmapEngine).toBe(true);
    expect(getContentWorkspace("roadmap").persistence).toBe("content-api");
    expect(getContentWorkspace("roadmap").contentType).toBe("Roadmap");
    expect(ADMIN_ROUTES.contentRoadmaps).toBe("/admin/content/roadmaps");
  });

  it("exposes admin roadmap routes", () => {
    const app = join(process.cwd(), "src", "app");
    expect(existsSync(join(app, "admin", "content", "roadmaps", "page.tsx"))).toBe(true);
    expect(existsSync(join(app, "admin", "content", "roadmaps", "new", "page.tsx"))).toBe(true);
    expect(existsSync(join(app, "admin", "content", "roadmaps", "[id]", "page.tsx"))).toBe(true);
  });

  it("wires roadmap API client paths", () => {
    const api = readFileSync(join(process.cwd(), "src/lib/api/content.ts"), "utf8");
    expect(api).toContain("`/admin/content/${encodeURIComponent(id)}/roadmap`");
    expect(api).toContain("`/admin/content/${encodeURIComponent(id)}/roadmap/steps`");
    expect(api).toContain("roadmap/steps/reorder");
  });

  it("renders topic editor and resource picker", () => {
    const topics = renderToStaticMarkup(
      <TopicEditor
        topics={[{ title: "Variables", description: null, order: 0 }]}
        onChange={() => undefined}
      />,
    );
    expect(topics).toContain("Variables");

    const resources = renderToStaticMarkup(
      <ResourcePicker
        resources={[
          {
            title: "MDN",
            url: "https://developer.mozilla.org",
            resourceType: "Article",
            order: 0,
          },
        ]}
        onChange={() => undefined}
      />,
    );
    expect(resources).toContain("MDN");
    expect(resources).toContain("content:");
  });

  it("renders ordering controls when multiple steps exist", () => {
    const html = renderToStaticMarkup(
      <DragDropOrdering
        steps={[
          {
            id: "a",
            title: "HTML",
            description: null,
            order: 0,
            estimatedHours: 10,
            projectTitle: null,
            projectDescription: null,
            topics: [],
            resources: [],
          },
          {
            id: "b",
            title: "JS",
            description: null,
            order: 1,
            estimatedHours: 20,
            projectTitle: null,
            projectDescription: null,
            topics: [],
            resources: [],
          },
        ]}
        onReorder={async () => undefined}
      />,
    );
    expect(html).toContain("HTML");
    expect(html).toContain("JS");
  });
});
