import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { PromptLabCategoryBar } from "@/components/prompt-lab/prompt-lab-category-bar";
import { PROMPT_LAB_QUICK_FILTERS } from "@/data/prompt-lab";

describe("prompt lab category bar", () => {
  it("renders the quick filter labels from the reference", () => {
    const html = renderToStaticMarkup(
      <PromptLabCategoryBar active="all" onSelect={() => undefined} />,
    );
    for (const item of PROMPT_LAB_QUICK_FILTERS) {
      expect(html).toContain(item.label);
    }
  });
});
