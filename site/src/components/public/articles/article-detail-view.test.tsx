import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { ArticleDetailView } from "@/components/public/articles/article-detail-view";
import type { ContentDetailDto } from "@/lib/api/content";
import { resolveMediaUrl } from "@/lib/admin/media/media-mappers";

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
    expect(html).toContain("اطلاعات مقاله");
    expect(html).toContain("اشتراک‌گذاری");
  });

  it("keeps site public news covers site-relative", () => {
    expect(resolveMediaUrl("/news/cover-claude.png")).toBe("/news/cover-claude.png");
    const html = renderToStaticMarkup(
      <ArticleDetailView
        article={{
          ...baseArticle,
          type: "News",
          slug: "claude-terminal-agent",
          coverImage: "/news/cover-claude.png",
          title: "Claude چیست؟ نگاهی به Terminal Agent",
        }}
      />,
    );
    expect(html).toContain('src="/news/cover-claude.png"');
    expect(html).toContain("در این خبر");
    expect(html).toContain("اطلاعات خبر");
    expect(html).not.toContain("برای محتوای کامل CMS");
  });

  it("falls back to title placeholder when coverImage is missing", () => {
    const html = renderToStaticMarkup(<ArticleDetailView article={baseArticle} />);
    expect(html).toContain("تست");
    expect(html).toContain("aspect-video");
  });

  it("renders three-column shell classes for desktop", () => {
    const html = renderToStaticMarkup(<ArticleDetailView article={baseArticle} />);
    expect(html).toContain("xl:grid-cols-[220px_minmax(0,1fr)_220px]");
    expect(html).toContain("مطالب مرتبط");
    expect(html).toContain("هیچ خبر مهمی را از دست ندهید");
  });
});
