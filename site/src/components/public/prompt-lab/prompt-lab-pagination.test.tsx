import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { PromptLabPagination, promptLabPageCount } from "./prompt-lab-pagination";

describe("prompt lab pagination", () => {
  it("hides when a single page is enough", () => {
    const html = renderToStaticMarkup(
      <PromptLabPagination page={1} pageSize={8} total={8} onPageChange={() => undefined} />,
    );
    expect(html).toBe("");
    expect(promptLabPageCount(20, 8)).toBe(3);
  });

  it("renders previous and next controls", () => {
    const html = renderToStaticMarkup(
      <PromptLabPagination page={2} pageSize={8} total={24} onPageChange={() => undefined} />,
    );
    expect(html).toContain("صفحه‌بندی پرامپت‌ها");
    expect(html).toContain("قبلی");
    expect(html).toContain("بعدی");
  });
});
