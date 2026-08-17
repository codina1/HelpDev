import type { PublicPromptDetailsDto, PublicPromptListItemDto } from "@/lib/api/promptlab";
import { coverForPromptLabCategorySlug } from "@/lib/public/prompt-lab-covers";
import type { PromptLabAuthor, PromptLabDetail } from "@/lib/public/prompt-lab-detail-mock";
import { PROMPT_LAB_CATEGORIES, type PromptLabCardItem } from "@/lib/public/prompt-lab-mock";

export const PROMPT_LAB_LIBRARY_AUTHOR: PromptLabAuthor = {
  id: "helpdev",
  name: "HelpDev",
  role: "کتابخانه پرامپت",
  bio: "پرامپت‌های تاییدشده برای ساخت، طراحی و توسعه با هوش مصنوعی.",
  initials: "HD",
};

export function labelPromptLabCategory(slug: string, fallback: string): string {
  return PROMPT_LAB_CATEGORIES.find((item) => item.slug === slug)?.name ?? fallback;
}

export function toPromptLabCardItem(dto: PublicPromptListItemDto): PromptLabCardItem {
  const categorySlug = dto.category.slug;
  return {
    id: dto.id,
    slug: dto.slug,
    title: dto.title,
    description: dto.description?.trim() || dto.title,
    coverImage: dto.coverImage?.trim() || coverForPromptLabCategorySlug(categorySlug),
    aiModel: dto.aiModel.name,
    category: labelPromptLabCategory(categorySlug, dto.category.name),
    categorySlug,
    copyCount: dto.copyCount,
    viewCount: dto.views,
    featured: false,
    publishedAt: dto.publishedAt ?? "",
  };
}

export function toPromptLabDetail(dto: PublicPromptDetailsDto): PromptLabDetail {
  const card = toPromptLabCardItem(dto);
  const mediaType = dto.mediaType;
  const tags = uniqueTags([
    card.category,
    card.aiModel,
    labelPromptLabMediaType(mediaType),
  ]);
  return {
    ...card,
    author: PROMPT_LAB_LIBRARY_AUTHOR,
    content: dto.content,
    tags,
    mediaType,
  };
}

export function labelPromptLabMediaType(mediaType: string): string {
  const key = mediaType.trim().toLowerCase();
  if (key === "text") return "متن";
  if (key === "image") return "تصویر";
  if (key === "audio") return "صدا";
  if (key === "video") return "ویدئو";
  return mediaType;
}

function uniqueTags(values: string[]): string[] {
  const seen = new Set<string>();
  const tags: string[] = [];
  for (const value of values) {
    const tag = value.trim();
    if (!tag || seen.has(tag)) continue;
    seen.add(tag);
    tags.push(tag);
  }
  return tags;
}
