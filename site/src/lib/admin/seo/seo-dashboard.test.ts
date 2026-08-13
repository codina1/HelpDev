import { readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";
import { ADMIN_ROUTES } from "@/lib/admin/routes";

describe("Admin SEO dashboard routing", () => {
  it("exposes /admin/seo in centralized routes", () => {
    expect(ADMIN_ROUTES.seo).toBe("/admin/seo");
  });

  it("registers the SEO page under admin layout", () => {
    const pagePath = join(process.cwd(), "src/app/admin/seo/page.tsx");
    const source = readFileSync(pagePath, "utf8");
    expect(source).toContain("SeoDashboardWorkspace");
  });

  it("gives admin tables RTL-safe cell padding so text is not flush to the card", () => {
    const css = readFileSync(join(process.cwd(), "src/app/globals.css"), "utf8");
    expect(css).toMatch(/\.adm-table[\s\S]*?padding-inline:\s*1\.1rem/);
  });
});
