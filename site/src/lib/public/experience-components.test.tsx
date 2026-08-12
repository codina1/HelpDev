import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import {
  CommandSearchBox,
  EngineeringTimeline,
  FeatureShowcase,
  InteractiveNode,
  PremiumSectionHeader,
  SmartEmptyState,
} from "@/components/experience";

describe("Sprint 50C — experience components", () => {
  it("renders section header, timeline, showcase, node", () => {
    expect(
      renderToStaticMarkup(
        <PremiumSectionHeader
          title="ابزارهای مهندسی"
          description="سریع‌تر"
          href="/toolbox"
          ctaLabel="مشاهده همه ابزارها"
          titleId="t1"
        />,
      ),
    ).toContain("ابزارهای مهندسی");

    expect(
      renderToStaticMarkup(
        <EngineeringTimeline title="Path" nodes={[{ label: "A" }, { label: "B" }]} />,
      ),
    ).toContain("A");

    expect(
      renderToStaticMarkup(
        <FeatureShowcase items={[{ title: "یادگیری", description: "d", href: "/learning" }]} />,
      ),
    ).toContain("یادگیری");

    expect(
      renderToStaticMarkup(
        <InteractiveNode label="AI" description="هسته" style={{ left: "50%", top: "50%" }} />,
      ),
    ).toContain("AI");
  });

  it("CommandSearchBox exposes Ctrl+K and Ask HelpDev AI prompt", () => {
    const html = renderToStaticMarkup(<CommandSearchBox />);
    expect(html).toContain("Ctrl");
    expect(html).toContain("Ask HelpDev AI");
    expect(html).toContain("چطور معماری یک سیستم SaaS را طراحی کنم؟");
  });

  it("SmartEmptyState renders premium messaging", () => {
    const html = renderToStaticMarkup(
      <SmartEmptyState
        title="مسیر مهندسی شما هنوز ساخته نشده"
        description="با AI بسازید"
        ctaLabel="ساخت مسیر با AI"
        ctaHref="/learning/assistant"
      />,
    );
    expect(html).toContain("مسیر مهندسی شما هنوز ساخته نشده");
    expect(html).toContain("ساخت مسیر با AI");
  });
});
