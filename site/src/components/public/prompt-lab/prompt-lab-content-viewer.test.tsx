import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { PromptLabContentViewer } from "./prompt-lab-content-viewer";

describe("prompt lab content viewer", () => {
  it("renders a code-style block with copy control and no toast until copied", () => {
    const html = renderToStaticMarkup(
      <PromptLabContentViewer content={"Goal\nReview module boundaries"} />,
    );
    expect(html).toContain("متن پرامپت");
    expect(html).toContain("Review module boundaries");
    expect(html).toContain('aria-label="کپی پرامپت"');
    expect(html).toContain('dir="ltr"');
    expect(html).not.toContain("پرامپت با موفقیت کپی شد");
  });
});
