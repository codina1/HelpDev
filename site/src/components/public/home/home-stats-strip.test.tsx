import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { HomeStatsStrip } from "@/components/public/home/home-stats-strip";
import type { HomeStatItem } from "@/components/public/home/home-stat";

const ITEMS: HomeStatItem[] = [
  { id: "engineers", label: "مهندسان فعال", value: 12, icon: "engineers" },
  { id: "articles", label: "مقالات فنی", value: 8, icon: "articles" },
  { id: "paths", label: "مسیرهای یادگیری", value: 3, icon: "paths" },
  { id: "tools", label: "ابزارهای مهندسی", value: 5, icon: "tools" },
  { id: "questions", label: "پرسش‌های پاسخ‌داده‌شده", value: 0, icon: "questions" },
];

describe("homepage stats strip", () => {
  it("renders five reusable stats with glass chrome", () => {
    const html = renderToStaticMarkup(<HomeStatsStrip items={ITEMS} />);
    expect(html).toContain("مهندسان فعال");
    expect(html).toContain("مقالات فنی");
    expect(html).toContain("مسیرهای یادگیری");
    expect(html).toContain("ابزارهای مهندسی");
    expect(html).toContain("پرسش‌های پاسخ‌داده‌شده");
    expect(html).toContain("backdrop-blur");
    expect(html).toContain("flex-wrap");
    expect(html).toContain("home-stats-row");
    expect(html).toContain("home-stat-icon");
    expect(html).toContain("home-stat-value");
  });
});
