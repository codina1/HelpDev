import { renderToStaticMarkup } from "react-dom/server";
import { describe, expect, it } from "vitest";
import { ApiClientError } from "@/lib/api/errors";
import { EMPTY_SEO_FORM, type ContentFormValues } from "@/lib/admin/content/content-types";
import { SeoGooglePreview } from "@/components/admin/content/seo/seo-google-preview";
import { SeoSocialPreview } from "@/components/admin/content/seo/seo-social-preview";
import { SeoPanel } from "@/components/admin/content/seo/seo-panel";
import { SaveStatusIndicator } from "@/components/admin/content/editor/save-status";
import { MarkdownEditor } from "@/components/admin/content/editor/markdown-editor";
import { ContentPreviewPanel } from "@/components/admin/content/editor/content-preview-panel";

const noop = () => {};

describe("SeoGooglePreview", () => {
  it("uses the content title/excerpt as fallback when SEO fields are empty", () => {
    const html = renderToStaticMarkup(
      <SeoGooglePreview
        seoTitle=""
        contentTitle="عنوان واقعی"
        seoDescription=""
        excerpt="خلاصه واقعی"
        canonicalUrl=""
        slug="my-slug"
      />,
    );
    expect(html).toContain("عنوان واقعی");
    expect(html).toContain("خلاصه واقعی");
    expect(html).toContain("/my-slug");
  });

  it("prefers the SEO title/description when provided", () => {
    const html = renderToStaticMarkup(
      <SeoGooglePreview
        seoTitle="عنوان سئو"
        contentTitle="عنوان واقعی"
        seoDescription="توضیح سئو"
        excerpt="خلاصه واقعی"
        canonicalUrl="https://helpdev.example/a"
        slug="my-slug"
      />,
    );
    expect(html).toContain("عنوان سئو");
    expect(html).toContain("توضیح سئو");
    expect(html).toContain("https://helpdev.example/a");
  });
});

describe("SeoSocialPreview", () => {
  it("renders a placeholder when no image is available", () => {
    const html = renderToStaticMarkup(
      <SeoSocialPreview
        seoTitle=""
        contentTitle="عنوان"
        seoDescription=""
        excerpt=""
        ogImage=""
        coverImage=""
        canonicalUrl=""
      />,
    );
    expect(html).toContain("بدون تصویر");
    expect(html).not.toContain("<img");
  });

  it("uses the cover image when the OG image is absent", () => {
    const html = renderToStaticMarkup(
      <SeoSocialPreview
        seoTitle=""
        contentTitle="عنوان"
        seoDescription=""
        excerpt=""
        ogImage=""
        coverImage="https://cdn.example.com/cover.png"
        canonicalUrl=""
      />,
    );
    expect(html).toContain("https://cdn.example.com/cover.png");
  });
});

describe("SeoPanel", () => {
  it("renders all SEO fields and its own save button, with no fabricated score", () => {
    const html = renderToStaticMarkup(
      <SeoPanel
        values={EMPTY_SEO_FORM}
        errors={{}}
        onChange={noop}
        onSave={noop}
        saveState="idle"
        contentTitle="عنوان"
        excerpt=""
        coverImage=""
        slug="a-slug"
        analysisStatus="idle"
        analysisReport={null}
        onAnalyze={noop}
      />,
    );
    expect(html).toContain("عنوان سئو");
    expect(html).toContain("توضیحات سئو");
    expect(html).toContain("آدرس کاننیکال");
    expect(html).toContain("کلمه کلیدی");
    expect(html).toContain("ذخیره سئو");
    // No fabricated SEO score/recommendation.
    expect(html).not.toContain("امتیاز");
  });

  it("surfaces API errors safely without leaking raw messages", () => {
    const error = new ApiClientError({ message: "raw-secret", status: 500 });
    const html = renderToStaticMarkup(
      <SeoPanel
        values={EMPTY_SEO_FORM}
        errors={{}}
        onChange={noop}
        onSave={noop}
        saveState="error"
        error={error}
        contentTitle="عنوان"
        excerpt=""
        coverImage=""
        slug="a-slug"
        analysisStatus="idle"
        analysisReport={null}
        onAnalyze={noop}
      />,
    );
    expect(html).not.toContain("raw-secret");
  });
});

describe("SaveStatusIndicator", () => {
  it("renders each non-idle state and nothing when idle", () => {
    expect(renderToStaticMarkup(<SaveStatusIndicator state="saving" />)).toContain("در حال ذخیره");
    expect(renderToStaticMarkup(<SaveStatusIndicator state="saved" />)).toContain("ذخیره شد");
    expect(renderToStaticMarkup(<SaveStatusIndicator state="unsaved" />)).toContain("ذخیره‌نشده");
    expect(renderToStaticMarkup(<SaveStatusIndicator state="error" />)).toContain("ناموفق");
    expect(renderToStaticMarkup(<SaveStatusIndicator state="idle" />)).toBe("");
  });
});

describe("MarkdownEditor", () => {
  it("renders the formatting toolbar", () => {
    const html = renderToStaticMarkup(
      <MarkdownEditor value="hello" onChange={noop} />,
    );
    expect(html).toContain("پررنگ");
    expect(html).toContain("پیوند");
    expect(html).toContain("hello");
  });
});

describe("ContentPreviewPanel", () => {
  const values: ContentFormValues = {
    title: "عنوان",
    slug: "a-slug",
    type: "Article",
    body: "**bold** and <script>alert(1)</script>",
    status: "Draft",
    excerpt: "خلاصه",
    coverImage: "",
  };

  it("offers desktop and mobile modes and renders markdown safely", () => {
    const html = renderToStaticMarkup(<ContentPreviewPanel values={values} bare />);
    expect(html).toContain("دسکتاپ");
    expect(html).toContain("موبایل");
    expect(html).toContain("<strong");
    expect(html).not.toContain("<script>alert(1)</script>");
    expect(html).toContain("خلاصه");
  });
});
