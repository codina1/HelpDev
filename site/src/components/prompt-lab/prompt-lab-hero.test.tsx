import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import {
  PROMPT_LAB_HERO_SUBTITLE,
  PROMPT_LAB_HERO_TITLE,
} from "@/data/prompt-lab";
import { PromptLabHero } from "@/components/prompt-lab/prompt-lab-hero";

describe("prompt lab hero", () => {
  it("renders title, subtitle, CTAs, and flask illustration", () => {
    const html = renderToStaticMarkup(<PromptLabHero />);
    expect(html).toContain(PROMPT_LAB_HERO_TITLE);
    expect(html).toContain(PROMPT_LAB_HERO_SUBTITLE);
    expect(html).toContain("پرامپت جدید");
    expect(html).toContain("ورود Prompt Lab");
    expect(html).toContain("/prompt-lab/hero-flask.png");
    expect(html).toContain("آزمایشگاه پرامپت");
  });
});
