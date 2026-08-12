import { readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";

describe("SEO dashboard API client", () => {
  it("calls GET /admin/seo/dashboard without storing tokens in source", () => {
    const source = readFileSync(join(process.cwd(), "src/lib/admin/seo/seo-api.ts"), "utf8");
    expect(source).toContain('path: "/admin/seo/dashboard"');
    expect(source).not.toMatch(/localStorage/);
    expect(source).not.toMatch(/sessionStorage/);
  });
});
