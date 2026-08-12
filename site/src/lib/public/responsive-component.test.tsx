import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { PublicSection } from "@/components/ui/public/v2/public-section";
import { KnowledgeCard } from "@/components/ui/public/v2/knowledge-card";
import { FeatureGrid } from "@/components/ui/public/v2/feature-grid";

describe("Sprint 50B — responsive component contracts", () => {
  it("containers use fluid padding and max-width classes", () => {
    const html = renderToStaticMarkup(
      <PublicContainer size="wide" className="test-c">
        x
      </PublicContainer>,
    );
    expect(html).toContain("px-4");
    expect(html).toContain("sm:px-5");
    expect(html).toContain("max-w-[1400px]");
  });

  it("section + cards expose responsive grid-friendly markup", () => {
    const section = renderToStaticMarkup(
      <PublicSection>
        <FeatureGrid
          items={[
            { title: "A", description: "d", href: "/a" },
            { title: "B", description: "d", href: "/b" },
          ]}
        />
      </PublicSection>,
    );
    expect(section).toContain("sm:grid-cols-2");
    expect(section).toContain("lg:grid-cols-4");

    const card = renderToStaticMarkup(
      <KnowledgeCard featured title="F" href="/articles/f" category="مقاله" />,
    );
    expect(card).toContain("sm:min-h-[280px]");
  });
});
