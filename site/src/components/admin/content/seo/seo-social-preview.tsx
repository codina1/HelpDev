"use client";

/**
 * A visual Open Graph / social card preview. Uses the OG image when provided,
 * otherwise the cover image; text falls back to SEO title/description then the
 * real content title/excerpt. No external calls, no image upload.
 */
export function SeoSocialPreview({
  seoTitle,
  contentTitle,
  seoDescription,
  excerpt,
  ogImage,
  coverImage,
  canonicalUrl,
}: {
  seoTitle: string;
  contentTitle: string;
  seoDescription: string;
  excerpt: string;
  ogImage: string;
  coverImage: string;
  canonicalUrl: string;
}) {
  const title = seoTitle.trim() || contentTitle.trim() || "عنوان محتوا";
  const description = seoDescription.trim() || excerpt.trim() || "بدون توضیحات";
  const image = ogImage.trim() || coverImage.trim();
  let host = "helpdev";
  const canonical = canonicalUrl.trim();
  if (canonical) {
    try {
      host = new URL(canonical).host || host;
    } catch {
      host = "helpdev";
    }
  }

  return (
    <div className="rounded-lg border border-[var(--adm-border)] bg-[var(--adm-surface-2)] p-3">
      <p className="adm-subtle mb-2 text-[11px] font-semibold">پیش‌نمایش شبکه اجتماعی</p>
      <div className="overflow-hidden rounded-lg border border-[var(--adm-border)]">
        <div className="flex aspect-[1.91/1] items-center justify-center bg-[var(--adm-surface-3)]">
          {image ? (
            // eslint-disable-next-line @next/next/no-img-element
            <img
              src={image}
              alt=""
              className="h-full w-full object-cover"
              referrerPolicy="no-referrer"
            />
          ) : (
            <span className="adm-subtle text-[11px]">بدون تصویر OG / کاور</span>
          )}
        </div>
        <div className="space-y-1 bg-[var(--adm-surface)] p-3">
          <p dir="ltr" className="adm-subtle text-start text-[11px] uppercase">
            {host}
          </p>
          <p className="line-clamp-1 text-[13px] font-bold text-[var(--adm-text)]">{title}</p>
          <p className="line-clamp-2 text-[12px] text-[var(--adm-text-muted)]">{description}</p>
        </div>
      </div>
    </div>
  );
}
