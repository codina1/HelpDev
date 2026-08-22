/**
 * @vitest-environment node
 */
import { existsSync, readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";

describe("content creation workspaces", () => {
  it("uses the article block editor for new articles", () => {
    const source = readFileSync(
      join(process.cwd(), "src/components/admin/content/workspaces/article/article-editor.tsx"),
      "utf8",
    );
    expect(source).toContain("ArticleBlockEditor");
    expect(source).not.toContain("ContentStudio");
    expect(source).not.toContain("WorkspaceCreateEditor");
  });

  it.each([
    ["news/news-editor.tsx", "News"],
    ["tool/tool-editor.tsx", "Tool"],
    ["roadmap/roadmap-editor.tsx", "Roadmap"],
    ["tutorial/tutorial-editor.tsx", "Course"],
  ])("uses the full Content Studio for %s", (file, type) => {
    const source = readFileSync(
      join(process.cwd(), "src/components/admin/content/workspaces", file),
      "utf8",
    );

    expect(source).toContain("ContentStudio");
    expect(source).toContain(`createType="${type}"`);
    expect(source).not.toContain("WorkspaceCreateEditor");
  });

  it("removes the obsolete simplified create editor", () => {
    expect(
      existsSync(
        join(
          process.cwd(),
          "src/components/admin/content/workspaces/workspace-create-editor.tsx",
        ),
      ),
    ).toBe(false);
  });

  it("shows Persian labels for article difficulty levels", () => {
    const source = readFileSync(
      join(
        process.cwd(),
        "src/components/admin/content/workspaces/article/article-settings-panel.tsx",
      ),
      "utf8",
    );

    expect(source).toContain('Beginner: "مبتدی"');
    expect(source).toContain('Intermediate: "متوسط"');
    expect(source).toContain('Advanced: "پیشرفته"');
    expect(source).toContain("{DIFFICULTY_LABELS[level]}");
  });
});
