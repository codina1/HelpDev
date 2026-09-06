import { renderToStaticMarkup } from "react-dom/server";
import { describe, expect, it } from "vitest";
import { FeaturedArticle } from "@/components/articles/featured-article";
import type { MarketplaceArticle } from "@/data/articles";
import {
  mapPublishedContentToMarketplace,
  resolveMarketplaceDescription,
  resolveMarketplaceTitle,
} from "@/lib/public/map-published-articles";
import type { ContentSummaryDto } from "@/lib/api/content";

const sample: MarketplaceArticle = {
  id: "1",
  slug: "cursor-ide",
  title: "Cursor IDE چیست؟ نسلی جدید با دید AI",
  description:
    "بررسی کامل Cursor 1.0، قابلیت‌های جدید، تفاوت‌ها با نسخه‌های قبلی و اینکه چرا این آینده برنامه‌نویسی با هوش مصنوعی است",
  category: "tools",
  categoryLabel: "Tools",
  level: "intermediate",
  author: "تیم HelpDev",
  authorInitials: "HD",
  readingMinutes: 17,
  views: 12000,
  publishedAt: "2026-08-20",
  featured: true,
  coverImage: "/articles/hero-book.png",
  coverTone: "from-[#1e1b4b] to-[#0f172a]",
};

describe("FeaturedArticle layout", () => {
  it("renders real title and summary and links the whole card", () => {
    const html = renderToStaticMarkup(<FeaturedArticle article={sample} />);
    expect(html).toContain(`/articles/${sample.slug}`);
    expect(html).toContain(sample.title);
    expect(html).toContain(sample.description);
    expect(html).toContain("مقاله ویژه");
    expect(html).toContain("مطالعه مقاله");
    expect(html).toContain("md:h-[260px]");
    expect(html).toContain("md:grid-cols-[minmax(0,55%)_minmax(0,45%)]");
    expect(html).toContain("mt-auto");
    expect(html).toContain("object-cover object-center");
    expect(html).toContain("from-[#8B5CF6]");
    expect(html).toContain("self-end");
    expect(html).not.toContain("md:h-[230px]");
    expect(html).not.toContain("md:h-[188px]");
    expect(html).not.toContain("border-s border-white/[0.14]");
  });

  it("keeps short content readable without collapsing title/summary", () => {
    const short: MarketplaceArticle = {
      ...sample,
      title: "تست 2",
      description: "نگاهی کوتاه به «تست 2» در HelpDev.",
    };
    const html = renderToStaticMarkup(<FeaturedArticle article={short} />);
    expect(html).toContain("تست 2");
    expect(html).toContain("نگاهی کوتاه به «تست 2» در HelpDev.");
  });
});

describe("marketplace description mapping", () => {
  it("prefers API excerpt over fallback copy", () => {
    expect(
      resolveMarketplaceDescription({
        title: "عنوان",
        slug: "slug",
        excerpt: "  خلاصه واقعی از CMS  ",
      }),
    ).toBe("خلاصه واقعی از CMS");
  });

  it("falls back to title-based copy when excerpt is empty", () => {
    expect(resolveMarketplaceTitle({ title: "  تست 2 ", slug: "test2" })).toBe("تست 2");
    expect(resolveMarketplaceDescription({ title: "تست 2", slug: "test2", excerpt: "   " })).toContain(
      "تست 2",
    );

    const mapped = mapPublishedContentToMarketplace([
      {
        id: "1",
        title: "تست React",
        slug: "test-react",
        type: "Article",
        status: "Published",
        views: 12,
        saves: 0,
        createdAt: "2026-08-24T00:00:00Z",
        excerpt: "این excerpt واقعی است.",
      } satisfies ContentSummaryDto,
    ]);
    expect(mapped[0]?.title).toBe("تست React");
    expect(mapped[0]?.description).toBe("این excerpt واقعی است.");
  });
});
