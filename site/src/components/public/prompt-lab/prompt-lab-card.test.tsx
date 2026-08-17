import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { PROMPT_LAB_PROMPTS } from "@/lib/public/prompt-lab-mock";
import { PromptLabCard } from "./prompt-lab-card";

describe("prompt lab card", () => {
  it("renders cover, title, description, badges, and counts", () => {
    const item = PROMPT_LAB_PROMPTS[0];
    const html = renderToStaticMarkup(<PromptLabCard item={item} />);
    expect(html).toContain(item.coverImage);
    expect(html).toContain(item.title);
    expect(html).toContain(item.description);
    expect(html).toContain(item.aiModel);
    expect(html).toContain(item.category);
    expect(html).toContain("کپی");
    expect(html).toContain("بازدید");
    expect(html).toContain(`/prompt-lab/${item.slug}`);
  });
});
