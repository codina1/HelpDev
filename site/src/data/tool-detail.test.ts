import { describe, expect, it } from "vitest";
import { getToolDetailBySlug, publicToolPath, resolveToolSlug } from "@/data/tool-detail";
import { MARKETPLACE_TOOLS } from "@/data/tools";

describe("tool detail catalog", () => {
  it("resolves marketplace tools and vscode aliases", () => {
    expect(resolveToolSlug("visual-studio-code")).toBe("vscode");
    const vscode = getToolDetailBySlug("visual-studio-code");
    expect(vscode?.slug).toBe("vscode");
    expect(vscode?.name).toBe("Visual Studio Code");
    expect(vscode?.features.length).toBeGreaterThan(0);
  });

  it("builds a detail model for every marketplace tool", () => {
    for (const tool of MARKETPLACE_TOOLS) {
      const detail = getToolDetailBySlug(tool.slug);
      expect(detail).not.toBeNull();
      expect(detail?.slug).toBe(tool.slug);
      expect(publicToolPath(tool.slug)).toBe(`/tools/${tool.slug}`);
    }
  });

  it("returns null for unknown tools", () => {
    expect(getToolDetailBySlug("missing-tool")).toBeNull();
  });
});
