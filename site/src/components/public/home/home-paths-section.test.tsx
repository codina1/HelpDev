import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import {
  LEARNING_PATH_ITEMS,
  LEARNING_PATH_ICON_SLOTS,
  LearningPathsSection,
} from "@/components/public/home/LearningPathsSection";
import { HomePathsSection } from "@/components/public/home/home-paths-section";

describe("homepage learning paths", () => {
  it("renders five glass cards with unique icons, difficulty, and progress", () => {
    const html = renderToStaticMarkup(<LearningPathsSection />);
    expect(html).toContain("مسیرهای یادگیری");
    expect(html).toContain("مشاهده همه مسیرها");
    expect(html).toContain("grid-cols-1");
    expect(html).toContain("sm:grid-cols-3");
    expect(html).toContain("lg:grid-cols-5");
    expect(html).toContain("rounded-[18px]");
    expect(html).toContain("border-[rgba(255,255,255,0.08)]");
    expect(html).toContain("hover:-translate-y-[6px]");
    expect(html).toContain('role="progressbar"');
    expect(html).toContain("dir=\"rtl\"");
    expect(LEARNING_PATH_ITEMS.map((item) => item.title)).toEqual([
      "AI Engineer",
      "Backend Developer",
      ".NET Developer",
      "DevOps Engineer",
      "Frontend Developer",
    ]);
    expect(LEARNING_PATH_ITEMS.map((item) => item.lessons)).toEqual([
      "28 درس",
      "32 درس",
      "24 درس",
      "27 درس",
      "30 درس",
    ]);
    expect(LEARNING_PATH_ITEMS.map((item) => item.progress)).toEqual([60, 40, 55, 65, 75]);
    for (const item of LEARNING_PATH_ITEMS) {
      expect(html).toContain(item.title);
      expect(html).toContain(item.description);
      expect(html).toContain(item.lessons);
      expect(html).toContain(item.difficulty);
      expect(html).toContain(item.iconSrc);
      expect(html).toContain(`data-icon-slot="${item.id}"`);
    }
    expect(Object.values(LEARNING_PATH_ICON_SLOTS)).toHaveLength(5);
    expect(new Set(Object.values(LEARNING_PATH_ICON_SLOTS)).size).toBe(5);
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
