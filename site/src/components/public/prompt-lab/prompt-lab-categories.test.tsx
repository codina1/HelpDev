import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { PROMPT_LAB_CATEGORIES } from "@/lib/public/prompt-lab-mock";
import { PromptLabCategories } from "./prompt-lab-categories";

describe("prompt lab categories", () => {
  it("renders the seven required category labels", () => {
    const html = renderToStaticMarkup(
      <PromptLabCategories selectedSlug={null} onSelect={() => undefined} />,
    );
    for (const category of PROMPT_LAB_CATEGORIES) {
      expect(html).toContain(category.name);
    }
    expect(html).toContain("دسته‌بندی پرامپت‌ها");
  });
});
