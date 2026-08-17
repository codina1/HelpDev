import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import {
  PROMPT_LAB_HERO_SUBTITLE,
  PROMPT_LAB_HERO_TITLE,
  PromptLabHero,
} from "./prompt-lab-hero";

describe("prompt lab hero", () => {
  it("renders title, subtitle, search, CTA, and HelpDev AI visual", () => {
    const html = renderToStaticMarkup(
      <PromptLabHero query="" onQueryChange={() => undefined} onSearch={() => undefined} onExplore={() => undefined} />,
    );
    expect(html).toContain(PROMPT_LAB_HERO_TITLE);
    expect(html).toContain(PROMPT_LAB_HERO_SUBTITLE);
    expect(html).toContain("جستجوی پرامپت");
    expect(html).toContain("کاوش پرامپت‌ها");
    expect(html).toContain("HelpDev AI");
    expect(html).toContain('role="search"');
    expect(html).not.toContain("آخرین مقالات");
  });
});
