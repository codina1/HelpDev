import { createContent as createContentV1 } from "@/lib/api/content";

export type ContentTypeOption =
  | "News"
  | "Article"
  | "RoadmapStep"
  | "Tool"
  | "Prompt"
  | "Course";

export type ContentStatusOption = "Draft" | "Published";

export type CreateContentRequest = {
  title: string;
  slug: string;
  body: string;
  type: ContentTypeOption;
  status: ContentStatusOption;
};

export type ContentDetail = {
  id: string;
  title: string;
  slug: string;
  body: string;
  type: string;
  authorId: string;
  status: string;
  views: number;
  saves: number;
  createdAt: string;
};

export const CONTENT_TYPE_OPTIONS: Array<{ value: ContentTypeOption; label: string }> = [
  { value: "Article", label: "مقاله" },
  { value: "News", label: "خبر" },
  { value: "Course", label: "دوره" },
  { value: "RoadmapStep", label: "گام نقشه راه" },
  { value: "Tool", label: "ابزار" },
  { value: "Prompt", label: "پرامپت" },
];

export function slugifyTitle(title: string): string {
  const base = title
    .trim()
    .toLowerCase()
    .replace(/\s+/g, "-")
    .replace(/[^a-z0-9-]/g, "")
    .replace(/-+/g, "-")
    .replace(/^-|-$/g, "");

  if (base.length >= 2) return base.slice(0, 80);

  const stamp = Date.now().toString(36);
  return `post-${stamp}`;
}

export async function createContent(
  token: string,
  request: CreateContentRequest,
): Promise<ContentDetail> {
  const data = await createContentV1(token, request);
  return {
    id: String(data.id),
    title: String(data.title ?? ""),
    slug: String(data.slug ?? ""),
    body: String(data.body ?? ""),
    type: String(data.type ?? ""),
    authorId: String(data.authorId ?? ""),
    status: String(data.status ?? ""),
    views: Number(data.views ?? 0),
    saves: Number(data.saves ?? 0),
    createdAt: String(data.createdAt ?? ""),
  };
}
