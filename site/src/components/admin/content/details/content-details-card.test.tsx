import { renderToStaticMarkup } from "react-dom/server";
import { describe, expect, it } from "vitest";
import { ContentDetailsCard } from "@/components/admin/content/details/content-details-card";
import type { ContentDetail } from "@/lib/admin/content/content-types";

const content: ContentDetail = {
  id: "content-1",
  title: "خبر آزمایشی",
  slug: "test-news",
  body: "متن خبر",
  coverImage: "https://helpdevapi.liara.run/media/2026/08/cover.png",
  type: "News",
  typeLabel: "خبر",
  authorId: "author-1",
  status: "Draft",
  statusLabel: "پیش‌نویس",
  views: 0,
  saves: 0,
  createdAt: "2026-08-17T00:00:00Z",
};

describe("ContentDetailsCard", () => {
  it("shows the article or news cover in the content preview", () => {
    const html = renderToStaticMarkup(<ContentDetailsCard content={content} />);

    expect(html).toContain(content.coverImage);
    expect(html).toContain(`تصویر کاور ${content.title}`);
  });

  it("shows a missing-cover message when there is no image", () => {
    const html = renderToStaticMarkup(
      <ContentDetailsCard content={{ ...content, coverImage: "" }} />,
    );

    expect(html).not.toContain("<img");
    expect(html).toContain("تصویر کاور برای این محتوا تنظیم نشده است.");
  });
});
