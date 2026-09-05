import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { ArticleDetailView } from "@/components/public/articles/article-detail-view";
import type { ContentDetailDto } from "@/lib/api/content";

const baseArticle: ContentDetailDto = {
  id: "1",
  title: "تست",
  slug: "test",
  type: "Article",
  status: "Published",
  views: 0,
  saves: 0,
  createdAt: "2026-08-24T00:00:00Z",
  body: "## مقدمه\n\nبدنه",
  authorId: "11111111-1111-1111-1111-111111111111",
};

describe("ArticleDetailView cover", () => {
  it("renders the resolved cover image when coverImage is set", () => {
    const html = renderToStaticMarkup(
      <ArticleDetailView
        article={{
          ...baseArticle,
          coverImage: "https://api.helpdev.ir/media/2026/08/cover.png",
        }}
      />,
    );

    expect(html).toContain('src="https://api.helpdev.ir/media/2026/08/cover.png"');
    expect(html).toContain("تست");
    expect(html).toContain("در این مقاله");
    expect(html).toContain("درباره نویسنده");
  });

  it("falls back to the gradient placeholder when coverImage is missing", () => {
    const html = renderToStaticMarkup(<ArticleDetailView article={baseArticle} />);

    expect(html).toContain("bg-gradient-to-bl");
    expect(html).toContain("aspect-[16/7]");
  });

  it("renders three-column shell classes for desktop", () => {
    const html = renderToStaticMarkup(<ArticleDetailView article={baseArticle} />);
    expect(html).toContain("xl:grid-cols-[260px_minmax(0,1fr)_260px]");
    expect(html).toContain("برچسب‌ها");
    expect(html).toContain("ابزارهای مرتبط");
  });
});
