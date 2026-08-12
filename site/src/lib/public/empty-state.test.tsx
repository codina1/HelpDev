import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { SmartEmptyState } from "@/components/experience/smart-empty-state";

describe("Sprint 50C — empty state", () => {
  it("roadmap empty copy matches product language", () => {
    const html = renderToStaticMarkup(
      <SmartEmptyState
        title="مسیر مهندسی شما هنوز ساخته نشده"
        description="desc"
        ctaLabel="ساخت مسیر با AI"
        ctaHref="/learning/assistant"
      />,
    );
    expect(html).toContain("مسیر مهندسی شما هنوز ساخته نشده");
  });

  it("recommendation empty copy encourages profile, not fake data", () => {
    const html = renderToStaticMarkup(
      <SmartEmptyState
        title="پیشنهادی آماده نیست"
        description="با چند اطلاعات ساده، HelpDev مسیر مناسب شما را پیدا می‌کند"
        ctaLabel="تنظیم ترجیحات"
        ctaHref="/learning/profile"
      />,
    );
    expect(html).toContain("با چند اطلاعات ساده");
    expect(html).not.toContain("fake");
  });
});
