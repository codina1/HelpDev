import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import {
  LEARNING_PATH_ITEMS,
  LEARNING_PATH_ICON_SLOTS,
  LearningPathsSection,
} from "@/components/public/home/LearningPathsSection";
import { HomePathsSection } from "@/components/public/home/home-paths-section";

describe("homepage learning paths", () => {
  it("renders five compact 220×130 cards with icon slots and progress", () => {
    const html = renderToStaticMarkup(<LearningPathsSection />);
    expect(html).toContain("مسیرهای یادگیری");
    expect(html).toContain("مشاهده همه مسیرها");
    expect(html).toContain("grid-cols-1");
    expect(html).toContain("sm:grid-cols-3");
    expect(html).toContain("lg:grid-cols-5");
    expect(html).toContain("h-[130px]");
    expect(html).toContain("max-w-[220px]");
    expect(html).toContain("rounded-[18px]");
    expect(html).toContain("bg-[#0B1224]");
    expect(html).toContain("hover:-translate-y-[6px]");
    expect(html).toContain('role="progressbar"');
    expect(html).toContain("درس");
    expect(LEARNING_PATH_ITEMS.map((item) => item.title)).toEqual([
      "AI Engineer",
      "Backend Developer",
      ".NET Developer",
      "DevOps Engineer",
      "Frontend Developer",
    ]);
    for (const item of LEARNING_PATH_ITEMS) {
      expect(html).toContain(item.title);
      expect(html).toContain(item.description);
      expect(html).toContain(item.iconSrc);
      expect(html).toContain(`data-icon-slot="${item.id}"`);
    }
    expect(Object.values(LEARNING_PATH_ICON_SLOTS)).toHaveLength(5);
  });

  it("uses a published roadmap slug when the title matches a track", () => {
    const html = renderToStaticMarkup(
      <LearningPathsSection
        roadmaps={[{ title: "Frontend Engineer Path", slug: "frontend-path" }]}
      />,
    );
    expect(html).toContain("/roadmap?slug=frontend-path");
  });

  it("keeps HomePathsSection as a thin alias", () => {
    const a = renderToStaticMarkup(<LearningPathsSection />);
    const b = renderToStaticMarkup(<HomePathsSection />);
    expect(a).toBe(b);
  });
});
