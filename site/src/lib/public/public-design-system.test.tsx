import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import {
  AICommandBox,
  AnimatedBackground,
  FeatureGrid,
  GlassCard,
  GlowButton,
  GradientText,
  KnowledgeCard,
  PremiumBadge,
  PublicContainer,
  PublicSection,
  RoadmapCard,
  ToolCard,
} from "@/components/ui/public/v2";

describe("Sprint 50B — public design system v2", () => {
  it("renders core layout primitives", () => {
    const html = renderToStaticMarkup(
      <PublicSection aria-label="sec">
        <PublicContainer>
          <GradientText as="h2">عنوان گرادیان</GradientText>
          <PremiumBadge variant="ai">AI</PremiumBadge>
          <GlassCard>
            <p>شیشه</p>
          </GlassCard>
        </PublicContainer>
      </PublicSection>,
    );
    expect(html).toContain("عنوان گرادیان");
    expect(html).toContain("شیشه");
    expect(html).toContain("AI");
  });

  it("renders Knowledge / Tool / Roadmap cards", () => {
    expect(
      renderToStaticMarkup(
        <KnowledgeCard title="مقاله" href="/articles/a" category="مقاله" readingTime="۵ دقیقه" />,
      ),
    ).toContain("/articles/a");

    expect(renderToStaticMarkup(<ToolCard title="Cursor" href="/tools/cursor" category="AI" />)).toContain(
      "Cursor",
    );

    expect(
      renderToStaticMarkup(
        <RoadmapCard
          title="Frontend"
          href="/roadmap"
          nodes={[{ label: "HTML" }, { label: "React" }]}
        />,
      ),
    ).toContain("HTML");
  });

  it("renders AICommandBox, FeatureGrid, GlowButton, AnimatedBackground", () => {
    expect(
      renderToStaticMarkup(<AICommandBox onOpenPalette={() => undefined} />),
    ).toContain("Ask HelpDev");

    expect(
      renderToStaticMarkup(
        <FeatureGrid
          items={[{ title: "یادگیری", description: "desc", href: "/learning", accent: "ai" }]}
        />,
      ),
    ).toContain("یادگیری");

    expect(renderToStaticMarkup(<GlowButton href="/learning">شروع</GlowButton>)).toContain("شروع");
    expect(renderToStaticMarkup(<AnimatedBackground variant="hero" />)).toContain("aria-hidden");
  });
});
