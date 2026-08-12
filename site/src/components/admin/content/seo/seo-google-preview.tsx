"use client";

/**
 * A purely visual Google-style search result preview. No Google API is called;
 * this only mirrors what the author has typed. Falls back from SEO fields to the
 * real content title/excerpt so the card always reflects genuine data.
 */
export function SeoGooglePreview({
  seoTitle,
  contentTitle,
  seoDescription,
  excerpt,
  canonicalUrl,
  slug,
}: {
  seoTitle: string;
  contentTitle: string;
  seoDescription: string;
  excerpt: string;
  canonicalUrl: string;
  slug: string;
}) {
  const title = seoTitle.trim() || contentTitle.trim() || "عنوان محتوا";
  const description =
    seoDescription.trim() || excerpt.trim() || "توضیحاتی برای نمایش در نتایج جست‌وجو وجود ندارد.";
  const displayUrl = canonicalUrl.trim() || (slug.trim() ? `/${slug.trim()}` : "/…");

  return (
    <div className="rounded-lg border border-[var(--adm-border)] bg-[var(--adm-surface-2)] p-3">
      <p className="adm-subtle mb-2 text-[11px] font-semibold">پیش‌نمایش نتیجه جست‌وجو</p>
      <div dir="ltr" className="text-start">
        <p className="truncate text-[12px] text-[var(--adm-success)]">{displayUrl}</p>
        <p className="mt-0.5 line-clamp-1 text-[18px] leading-6 text-[var(--adm-info)]">
          {title}
        </p>
        <p className="mt-1 line-clamp-2 text-[12px] leading-5 text-[var(--adm-text-muted)]">
          {description}
        </p>
      </div>
    </div>
  );
}
