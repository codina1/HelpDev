import { readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";
import { ADMIN_ROUTES } from "@/lib/admin/routes";

describe("Content analytics frontend contracts", () => {
  it("exposes /admin/analytics/content route", () => {
    expect(ADMIN_ROUTES.analyticsContent).toBe("/admin/analytics/content");
  });

  it("API client targets real admin analytics content endpoints", () => {
    const source = readFileSync(
      join(process.cwd(), "src/lib/admin/analytics/content/content-analytics-api.ts"),
      "utf8",
    );
    expect(source).toContain('path: "/admin/analytics/content"');
    expect(source).toContain("/admin/analytics/top-content");
    expect(source).toContain("/admin/analytics/content-health");
    expect(source).not.toMatch(/localStorage/);
    expect(source).not.toMatch(/Google Analytics|Matomo|fakeGrowth/i);
  });

  it("dashboard components avoid fake score/ranking wording", () => {
    const files = [
      "src/components/admin/analytics/content/content-analytics-dashboard.tsx",
      "src/components/admin/analytics/content/content-health-panel.tsx",
      "src/components/admin/analytics/content/metric-card.tsx",
    ].map((rel) => readFileSync(join(process.cwd(), rel), "utf8"));

    for (const source of files) {
      expect(source).not.toMatch(/امتیاز سئو|رتبه‌بندی|پیش‌بینی|Google Analytics/i);
    }
  });

  it("registers content analytics page under admin", () => {
    const page = readFileSync(
      join(process.cwd(), "src/app/admin/analytics/content/page.tsx"),
      "utf8",
    );
    expect(page).toContain("ContentAnalyticsDashboard");
  });

  it("content detail tabs include analytics", () => {
    const tabs = readFileSync(
      join(process.cwd(), "src/components/admin/content/details/content-detail-tabs.tsx"),
      "utf8",
    );
    expect(tabs).toContain('"analytics"');
    expect(tabs).toContain("/analytics");
  });
});
