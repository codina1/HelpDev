import { renderToStaticMarkup } from "react-dom/server";
import { describe, expect, it } from "vitest";
import { TopContentTable } from "@/components/admin/analytics/content/top-content-table";
import { ContentHealthPanel } from "@/components/admin/analytics/content/content-health-panel";
import { ContentPerformanceCard } from "@/components/admin/analytics/content/content-performance-card";

describe("Content analytics UI states", () => {
  it("top table empty state is honest", () => {
    const html = renderToStaticMarkup(<TopContentTable items={[]} />);
    expect(html).toContain("هنوز بازدیدی");
    expect(html).not.toContain("امتیاز");
  });

  it("top table renders real view counts", () => {
    const html = renderToStaticMarkup(
      <TopContentTable
        items={[
          {
            contentId: "c1",
            title: "مقاله",
            slug: "article",
            views: 12,
            metrics: [],
            generatedAtUtc: "2026-07-01T00:00:00Z",
          },
        ]}
      />,
    );
    expect(html).toContain("مقاله");
    expect(html).toContain("۱۲");
  });

  it("health panel lists reasons without scores", () => {
    const html = renderToStaticMarkup(
      <ContentHealthPanel
        items={[
          {
            contentId: "c1",
            title: "پیش‌نویس",
            status: "Draft",
            healthStatus: "NeedsAttention",
            reasons: ["Missing SEO title"],
            viewsInPeriod: 0,
            revisionCount: 1,
            updatedAtUtc: "2026-01-01T00:00:00Z",
          },
        ]}
      />,
    );
    expect(html).toContain("Missing SEO title");
    expect(html).not.toMatch(/Score|امتیاز/);
  });

  it("performance card empty state when null", () => {
    const html = renderToStaticMarkup(<ContentPerformanceCard performance={null} />);
    expect(html).toContain("متریک ذخیره‌شده‌ای وجود ندارد");
  });
});
