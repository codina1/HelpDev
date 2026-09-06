import { renderToStaticMarkup } from "react-dom/server";
import { describe, expect, it } from "vitest";
import { FeaturedArticle } from "@/components/articles/featured-article";
import type { MarketplaceArticle } from "@/data/articles";

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
  it("uses full-width fixed-height desktop split with mt-auto CTA", () => {
    const html = renderToStaticMarkup(<FeaturedArticle article={sample} />);
    expect(html).toContain("w-full");
    expect(html).toContain("md:h-[230px]");
    expect(html).toContain("md:grid-cols-[minmax(0,55%)_minmax(0,45%)]");
    expect(html).toContain("mt-auto");
    expect(html).toContain("justify-end");
    expect(html).toContain("object-cover");
    expect(html).toContain("absolute inset-0 h-full w-full");
    expect(html).toContain("مقاله ویژه");
    expect(html).toContain("مطالعه مقاله");
    expect(html).toContain('class="flex h-full min-h-0 min-w-0 flex-col p-6 sm:p-7 md:h-full"');
    expect(html).not.toContain("justify-center px-");
    expect(html).not.toContain("max-w-xl");
    expect(html).not.toContain("max-w-lg");
    expect(html).not.toContain("md:h-[188px]");
  });
});
