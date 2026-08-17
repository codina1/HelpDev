import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import {
  PromptCard,
  PromptCardSkeleton,
  labelPromptCardMediaType,
  type PromptCardModel,
} from "./prompt-card";

const SAMPLE: PromptCardModel = {
  title: "بازبینی مرز ماژول",
  description: "پرامپت بررسی قرارداد دامنه و نشت زیرساخت.",
  category: "کدنویسی",
  aiModel: "Claude",
  mediaType: "Text",
  coverImage: "/home/cover-architecture.svg",
  viewCount: 1240,
  copyText: "Review module boundaries.",
};

describe("PromptCard", () => {
  it("renders cover, title, description, badges, copy, bookmark, and views", () => {
    const html = renderToStaticMarkup(<PromptCard item={SAMPLE} />);
    expect(html).toContain(SAMPLE.coverImage);
    expect(html).toContain(SAMPLE.title);
    expect(html).toContain(SAMPLE.description);
    expect(html).toContain(SAMPLE.category);
    expect(html).toContain(SAMPLE.aiModel);
    expect(html).toContain("متن");
    expect(html).toContain('aria-label="کپی پرامپت"');
    expect(html).toContain('aria-label="افزودن نشان"');
    expect(html).toContain("بازدید");
    expect(html).toContain('data-prompt-card="ready"');
    expect(html).toContain('dir="rtl"');
  });

  it("uses the empty-image state when cover is missing", () => {
    const html = renderToStaticMarkup(
      <PromptCard item={{ ...SAMPLE, coverImage: null, mediaType: "Image" }} />,
    );
    expect(html).toContain('data-prompt-card="empty-image"');
    expect(html).toContain("بدون تصویر");
    expect(html).toContain("data-empty-cover");
    expect(html).not.toContain("<img");
    expect(html).toContain("تصویر");
  });

  it("renders the loading skeleton without prompt fields", () => {
    const html = renderToStaticMarkup(<PromptCard item={SAMPLE} loading />);
    expect(html).toContain('data-prompt-card="loading"');
    expect(html).toContain("aria-busy");
    expect(html).toContain("در حال بارگذاری پرامپت");
    expect(html).not.toContain(SAMPLE.title);
    expect(html).not.toContain('aria-label="کپی پرامپت"');
  });

  it("marks a bookmarked card and labels media types", () => {
    const html = renderToStaticMarkup(<PromptCard item={SAMPLE} bookmarked />);
    expect(html).toContain('aria-pressed="true"');
    expect(html).toContain("حذف نشان");
    expect(labelPromptCardMediaType("Video")).toBe("ویدئو");
    expect(labelPromptCardMediaType("audio")).toBe("صدا");
  });

  it("exposes a standalone skeleton for list placeholders", () => {
    const html = renderToStaticMarkup(<PromptCardSkeleton />);
    expect(html).toContain('data-prompt-card="loading"');
  });
});
