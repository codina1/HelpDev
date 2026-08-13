import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { ABOUT_MISSION_ITEMS, AboutMission } from "./about-mission";

describe("about page mission", () => {
  it("renders three glass mission cards with the required copy", () => {
    const html = renderToStaticMarkup(<AboutMission />);
    expect(html).toContain("ماموریت");
    for (const item of ABOUT_MISSION_ITEMS) {
      expect(html).toContain(item.title);
      expect(html).toContain(item.description);
    }
    expect(html).toContain("about-mission");
  });
});
