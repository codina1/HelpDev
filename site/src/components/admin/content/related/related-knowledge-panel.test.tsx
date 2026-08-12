import { renderToStaticMarkup } from "react-dom/server";
import { describe, expect, it, vi } from "vitest";
import { RelatedKnowledgePanel } from "./related-knowledge-panel";

vi.mock("@/components/auth", () => ({
  useAuth: () => ({ token: null }),
}));

describe("RelatedKnowledgePanel", () => {
  it("renders without fabricating related items", () => {
    const html = renderToStaticMarkup(<RelatedKnowledgePanel contentId="content-1" />);
    expect(html).toContain("دانش مرتبط");
    expect(html).toContain("پیوند خودکار ایجاد نمی‌شود");
    expect(html).not.toContain("similarity 0.99");
    expect(html).not.toContain("embedding");
  });
});
