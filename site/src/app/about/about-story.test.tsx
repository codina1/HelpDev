import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { ABOUT_STORY_STEPS, AboutStory } from "./about-story";

describe("about page story timeline", () => {
  it("renders the four story steps in a vertical timeline", () => {
    const html = renderToStaticMarkup(<AboutStory />);
    expect(html).toContain("داستان HelpDev");
    expect(html).toContain("about-story");
    for (const step of ABOUT_STORY_STEPS) {
      expect(html).toContain(step.title);
    }
    expect(html).toContain("۱");
    expect(html).toContain("۴");
  });
});
