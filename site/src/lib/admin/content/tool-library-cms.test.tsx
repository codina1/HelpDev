import { describe, expect, it } from "vitest";
import { existsSync, readFileSync } from "node:fs";
import { join } from "node:path";
import { renderToStaticMarkup } from "react-dom/server";
import { ToolFeaturesEditor } from "@/components/admin/content/workspaces/tool/tool-features-editor";
import { ToolAlternativesEditor } from "@/components/admin/content/workspaces/tool/tool-alternatives-editor";
import { ToolPreview } from "@/components/admin/content/workspaces/tool/tool-preview";
import { EMPTY_TOOL_FORM } from "@/components/admin/content/workspaces/tool/tool-form-types";
import { CONTENT_CAPABILITIES } from "@/lib/admin/content/content-api";
import { getContentWorkspace } from "@/lib/admin/content/registry";
import { ADMIN_ROUTES } from "@/lib/admin/routes";

describe("Sprint 48 — tool library frontend", () => {
  it("registers tool workspace on content-api with Tool type", () => {
    expect(CONTENT_CAPABILITIES.toolLibrary).toBe(true);
    expect(getContentWorkspace("tool").persistence).toBe("content-api");
    expect(getContentWorkspace("tool").contentType).toBe("Tool");
    expect(ADMIN_ROUTES.contentTools).toBe("/admin/content/tools");
    expect(ADMIN_ROUTES.contentToolsNew).toBe("/admin/content/tools/new");
  });

  it("exposes admin and public foundation routes", () => {
    const app = join(process.cwd(), "src", "app");
    expect(existsSync(join(app, "admin", "content", "tools", "page.tsx"))).toBe(true);
    expect(existsSync(join(app, "admin", "content", "tools", "new", "page.tsx"))).toBe(true);
    expect(existsSync(join(app, "admin", "content", "tools", "[id]", "page.tsx"))).toBe(true);
    expect(existsSync(join(app, "tools", "[slug]", "page.tsx"))).toBe(true);
  });

  it("wires tool API client paths", () => {
    const api = readFileSync(join(process.cwd(), "src/lib/api/content.ts"), "utf8");
    expect(api).toContain('path: "/admin/tools"');
    expect(api).toContain("`/admin/content/${encodeURIComponent(id)}/tool`");
    expect(api).toContain("`/admin/content/${encodeURIComponent(id)}/tool/features`");
  });

  it("renders feature editor with validation affordance", () => {
    const html = renderToStaticMarkup(
      <ToolFeaturesEditor
        features={[{ id: "f1", title: "AI Agent", description: null, order: 0 }]}
        onAdd={async () => undefined}
        onRemove={async () => undefined}
      />,
    );
    expect(html).toContain("AI Agent");
    expect(html).toContain("افزودن ویژگی");
  });

  it("renders alternatives editor and preview", () => {
    const altHtml = renderToStaticMarkup(
      <ToolAlternativesEditor
        items={[{ alternativeToolContentId: "11111111-1111-1111-1111-111111111111", order: 0 }]}
        onChange={() => undefined}
      />,
    );
    expect(altHtml).toContain("جایگزین");

    const preview = renderToStaticMarkup(
      <ToolPreview
        tool={{
          ...EMPTY_TOOL_FORM,
          toolName: "Cursor",
          officialWebsiteUrl: "https://cursor.com",
          pricingModel: "Freemium",
          platforms: ["Web", "Windows"],
        }}
        title="Cursor"
        body=""
      />,
    );
    expect(preview).toContain("Cursor");
    expect(preview).toContain("Freemium");
  });
});
