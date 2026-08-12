import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { FeatureShowcase } from "@/components/experience/feature-showcase";
import { KnowledgeGalaxy } from "@/components/experience/knowledge-galaxy";
import { PublicContainer } from "@/components/ui/public/v2/public-container";

describe("Sprint 50C — responsive layout contracts", () => {
  it("feature showcase uses responsive grid breakpoints", () => {
    const html = renderToStaticMarkup(
      <FeatureShowcase
        items={[
          { title: "A", description: "d", href: "/a" },
          { title: "B", description: "d", href: "/b" },
        ]}
      />,
    );
    expect(html).toContain("sm:grid-cols-2");
    expect(html).toContain("lg:grid-cols-4");
  });

  it("galaxy and container avoid fixed overflow traps", () => {
    const galaxy = renderToStaticMarkup(<KnowledgeGalaxy />);
    expect(galaxy).toContain("max-w-[440px]");
    expect(galaxy).toContain("w-full");

    const container = renderToStaticMarkup(<PublicContainer size="wide">x</PublicContainer>);
    expect(container).toContain("px-4");
    expect(container).toContain("w-full");
  });
});
