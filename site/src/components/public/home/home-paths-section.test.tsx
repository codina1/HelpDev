import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { HOME_PATH_ITEMS, HomePathsSection } from "@/components/public/home/home-paths-section";

describe("homepage learning paths", () => {
  it("renders five horizontal path cards with honest learner counts", () => {
    const html = renderToStaticMarkup(<HomePathsSection />);
    expect(html).toContain("مسیرهای یادگیری");
    expect(html).toContain("/home/");
    expect(html).toContain("min-[1440px]:grid-cols-5");
    expect(html).toContain("grid-cols-2");
    for (const item of HOME_PATH_ITEMS) {
      expect(html).toContain(item.title);
      expect(html).toContain(item.description);
    }
    expect(html).toContain("یادگیرنده");
    expect(html).toContain("۰");
    expect(html).toContain("/roadmap");
    expect(html).toContain("home-path-visual");
  });

  it("uses a published roadmap slug when the title matches a track", () => {
    const html = renderToStaticMarkup(
      <HomePathsSection
        roadmaps={[{ title: "Frontend Engineer Path", slug: "frontend-path" }]}
      />,
    );
    expect(html).toContain("/roadmap?slug=frontend-path");
  });
});
