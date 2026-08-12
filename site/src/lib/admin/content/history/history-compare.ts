import type { ContentRevisionSnapshot } from "@/lib/admin/content/history/history-types";

export type ComparedRevisionField = {
  key: string;
  label: string;
  left: string;
  right: string;
  changed: boolean;
};

function norm(value: string | null | undefined): string {
  return (value ?? "").trim();
}

function pair(
  key: string,
  label: string,
  left: string | null | undefined,
  right: string | null | undefined,
): ComparedRevisionField {
  const l = norm(left);
  const r = norm(right);
  return { key, label, left: l, right: r, changed: l !== r };
}

/**
 * Builds side-by-side comparable rows for title, body, excerpt, slug, cover, type, and SEO fields.
 * No external diff library — equality is line-normalized string compare.
 */
export function compareRevisionSnapshots(
  left: ContentRevisionSnapshot,
  right: ContentRevisionSnapshot,
): ComparedRevisionField[] {
  const seoLeft = left.seoMetadata;
  const seoRight = right.seoMetadata;

  return [
    pair("title", "عنوان", left.title, right.title),
    pair("slug", "اسلاگ", left.slug, right.slug),
    pair("excerpt", "خلاصه", left.excerpt, right.excerpt),
    pair("body", "متن", left.body, right.body),
    pair("coverImage", "تصویر کاور", left.coverImage, right.coverImage),
    pair("contentType", "نوع محتوا", left.contentType, right.contentType),
    pair("seoTitle", "عنوان سئو", seoLeft.seoTitle, seoRight.seoTitle),
    pair("seoDescription", "توضیح سئو", seoLeft.seoDescription, seoRight.seoDescription),
    pair("canonicalUrl", "آدرس canonical", seoLeft.canonicalUrl, seoRight.canonicalUrl),
    pair("ogImage", "تصویر OG", seoLeft.ogImage, seoRight.ogImage),
    pair("focusKeyword", "کلمه کلیدی", seoLeft.focusKeyword, seoRight.focusKeyword),
  ];
}

export function countChangedFields(fields: ComparedRevisionField[]): number {
  return fields.filter((field) => field.changed).length;
}
