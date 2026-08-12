import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import {
  AiCard,
  ArticleCard,
  Badge,
  Button,
  Card,
  EmptyState,
  ErrorState,
  Input,
  LoadingState,
  RoadmapCard,
  ToolCard,
} from "@/components/ui/ds";
import { dsColors, designSystem } from "@/lib/design-system";
import { ApiClientError } from "@/lib/api/errors";

describe("Sprint 50D-1 — design system foundation", () => {
  it("exposes premium AI theme tokens", () => {
    expect(dsColors.background.toLowerCase()).toBe("#060816");
    expect(dsColors.primary.toLowerCase()).toBe("#8b5cf6");
    expect(dsColors.secondary.toLowerCase()).toBe("#06b6d4");
    expect(designSystem.animations.hoverLift).toBe("ds-hover-lift");
  });

  it("renders primitives", () => {
    expect(renderToStaticMarkup(<Button>OK</Button>)).toContain("OK");
    expect(renderToStaticMarkup(<Badge variant="ai">AI</Badge>)).toContain("AI");
    expect(renderToStaticMarkup(<Card>Surface</Card>)).toContain("Surface");
    expect(renderToStaticMarkup(<Input aria-label="q" placeholder="search" />)).toContain("search");
  });

  it("renders card variants", () => {
    expect(
      renderToStaticMarkup(<ArticleCard title="Art" href="/articles/a" category="مقاله" />),
    ).toContain("/articles/a");
    expect(renderToStaticMarkup(<ToolCard title="Cursor" href="/toolbox" />)).toContain("Cursor");
    expect(
      renderToStaticMarkup(
        <RoadmapCard title="FE" href="/roadmap" nodes={[{ label: "React" }]} />,
      ),
    ).toContain("React");
    expect(renderToStaticMarkup(<AiCard title="دستیار" description="desc" />)).toContain("دستیار");
  });

  it("renders empty / loading / error states safely", () => {
    expect(renderToStaticMarkup(<EmptyState title="خالی" />)).toContain("خالی");
    expect(renderToStaticMarkup(<LoadingState label="بارگذاری" />)).toContain("بارگذاری");
    const error = renderToStaticMarkup(
      <ErrorState
        error={new ApiClientError({ message: "شکست", status: 500, correlationId: "corr-ds" })}
      />,
    );
    expect(error).toContain("corr-ds");
    expect(error).not.toContain("stack");
  });
});
