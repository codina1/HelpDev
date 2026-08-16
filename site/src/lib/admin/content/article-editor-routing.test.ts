/**
 * @vitest-environment node
 */
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";

describe("article creation workspace", () => {
  it("uses the full Content Studio instead of the simplified create editor", () => {
    const source = readFileSync(
      join(
        process.cwd(),
        "src/components/admin/content/workspaces/article/article-editor.tsx",
      ),
      "utf8",
    );

    expect(source).toContain("ContentStudio");
    expect(source).toContain('createType="Article"');
    expect(source).not.toContain("WorkspaceCreateEditor");
  });
});
