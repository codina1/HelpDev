import type { ContentSummaryDto } from "@/lib/api/content";
import { formatDateFa, labelForContentType } from "@/lib/admin/content/content-mappers";

export function isArticleType(type: string): boolean {
  const t = type.toLowerCase();
  return t === "article" || t === "news";
}

export function isRoadmapType(type: string): boolean {
  const t = type.toLowerCase();
  return t === "roadmap" || t === "roadmapstep";
}

export function isToolContentType(type: string): boolean {
  return type.toLowerCase() === "tool";
}

export function publicHrefForContent(item: Pick<ContentSummaryDto, "type" | "slug">): string {
  const t = item.type.toLowerCase();
  if (t === "tool") return `/tools/${encodeURIComponent(item.slug)}`;
  if (t === "roadmap" || t === "roadmapstep") {
    return `/roadmap?slug=${encodeURIComponent(item.slug)}`;
  }
  if (t === "course") return `/courses?slug=${encodeURIComponent(item.slug)}`;
  if (t === "prompt") return `/prompt-lab?slug=${encodeURIComponent(item.slug)}`;
  return `/articles/${encodeURIComponent(item.slug)}`;
}

export function contentMetaLine(item: ContentSummaryDto): string {
  const parts = [labelForContentType(item.type), formatDateFa(item.createdAt)].filter(Boolean);
  return parts.join(" · ");
}

export type TocHeading = {
  id: string;
  text: string;
  level: 2 | 3;
};

/**
 * Extracts h2/h3 headings from markdown-ish body for TOC foundation.
 * Safe for plain text bodies (returns empty).
 */
export function extractTocFromBody(body: string): TocHeading[] {
  const headings: TocHeading[] = [];
  const seen = new Map<string, number>();

  for (const line of body.split(/\r?\n/)) {
    const match = /^(#{2,3})\s+(.+?)\s*$/.exec(line.trim());
    if (!match) continue;
    const level = match[1].length === 2 ? 2 : 3;
    const text = match[2].replace(/[#*`_]/g, "").trim();
    if (!text) continue;
    headings.push(nextHeading(text, level, seen));
  }

  return headings;
}

/** Extracts h2/h3 headings from server-compiled article HTML. */
export function extractTocFromHtml(html: string): TocHeading[] {
  const headings: TocHeading[] = [];
  const seen = new Map<string, number>();
  const matches = html.matchAll(/<h([23])\b([^>]*)>([\s\S]*?)<\/h\1>/gi);
  for (const match of matches) {
    const level = Number(match[1]) === 3 ? 3 : 2;
    const attrId = /\bid="([^"]+)"/i.exec(match[2] ?? "")?.[1];
    const text = stripTags(match[3] ?? "").trim();
    if (!text) continue;
    const heading = nextHeading(text, level, seen);
    headings.push({ ...heading, id: attrId || heading.id });
  }
  return headings;
}

export function isBlockArticle(format: string | null | undefined, html: string | null | undefined): boolean {
  return (format ?? "").toLowerCase() === "blocks" && Boolean(html?.trim());
}

/** Extra client-side defense on server-compiled HTML (scripts/handlers stripped). */
export function sanitizeArticleHtml(html: string): string {
  return html
    .replace(/<script[\s\S]*?>[\s\S]*?<\/script>/gi, "")
    .replace(/\son\w+\s*=\s*("[^"]*"|'[^']*'|[^\s>]+)/gi, "")
    .replace(/javascript:/gi, "");
}

function nextHeading(text: string, level: 2 | 3, seen: Map<string, number>): TocHeading {
  let id = slugifyHeading(text);
  const count = (seen.get(id) ?? 0) + 1;
  seen.set(id, count);
  if (count > 1) id = `${id}-${count}`;
  return { id, text, level };
}

function stripTags(value: string): string {
  return value.replace(/<[^>]+>/g, "").replace(/&nbsp;/g, " ").trim();
}

function slugifyHeading(text: string): string {
  const ascii = text
    .toLowerCase()
    .replace(/[^a-z0-9\u0600-\u06FF]+/g, "-")
    .replace(/^-+|-+$/g, "");
  return ascii || "section";
}

/** Injects heading ids into markdown body for in-page anchors (display foundation). */
export function enrichBodyWithHeadingIds(body: string, toc: TocHeading[]): string {
  if (toc.length === 0) return body;
  let index = 0;
  return body
    .split(/\r?\n/)
    .map((line) => {
      const match = /^(#{2,3})\s+(.+?)\s*$/.exec(line.trim());
      if (!match || index >= toc.length) return line;
      const heading = toc[index++];
      return `${match[1]} ${heading.text} {#${heading.id}}`;
    })
    .join("\n");
}
