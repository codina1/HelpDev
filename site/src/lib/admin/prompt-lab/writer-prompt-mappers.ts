import type { WriterPromptListItemDto, WriterPromptPageDto } from "@/lib/api/promptlab-writer";
import { isValidSlug, slugify } from "@/lib/admin/content/content-mappers";
import type {
  WriterPromptFormErrors,
  WriterPromptFormValues,
  WriterPromptListItem,
  WriterPromptMediaType,
  WriterPromptPagedResult,
  WriterPromptStatus,
} from "./writer-prompt-types";
import {
  WRITER_PROMPT_LIMITS,
  WRITER_PROMPT_MEDIA_TYPES,
  WRITER_PROMPT_STATUSES,
} from "./writer-prompt-types";

export const WRITER_PROMPT_STATUS_LABELS: Record<WriterPromptStatus, string> = {
  Draft: "پیش‌نویس",
  Submitted: "در انتظار بررسی",
  Approved: "منتشرشده",
  Rejected: "ردشده",
};

export const WRITER_PROMPT_STATUS_BADGE_CLASS: Record<WriterPromptStatus, string> = {
  Draft: "bg-[var(--adm-warning-soft)] text-[var(--adm-warning)]",
  Submitted: "bg-[var(--adm-info-soft)] text-[var(--adm-info)]",
  Approved: "bg-[var(--adm-success-soft)] text-[var(--adm-success)]",
  Rejected: "bg-[var(--adm-danger-soft)] text-[var(--adm-danger)]",
};

export function labelForWriterPromptStatus(status: string): string {
  return WRITER_PROMPT_STATUS_LABELS[status as WriterPromptStatus] ?? status;
}

export function isKnownWriterPromptStatus(status: string): status is WriterPromptStatus {
  return (WRITER_PROMPT_STATUSES as readonly string[]).includes(status);
}

export function mapWriterPromptListItem(raw: WriterPromptListItemDto): WriterPromptListItem {
  const status = isKnownWriterPromptStatus(raw.status) ? raw.status : "Draft";
  return {
    id: raw.id,
    title: raw.title,
    slug: raw.slug,
    status,
    statusLabel: labelForWriterPromptStatus(status),
    views: raw.views,
    copyCount: raw.copyCount,
    createdAt: raw.createdAt,
  };
}

export function mapWriterPromptPagedResult(raw: WriterPromptPageDto): WriterPromptPagedResult {
  const totalPages = raw.pageSize > 0 ? Math.max(1, Math.ceil(raw.total / raw.pageSize)) : 1;
  return {
    page: raw.page,
    pageSize: raw.pageSize,
    totalCount: raw.total,
    totalPages: raw.total === 0 ? 0 : totalPages,
    items: raw.items.map(mapWriterPromptListItem),
  };
}

export const WRITER_PROMPT_MEDIA_TYPE_LABELS: Record<WriterPromptMediaType, string> = {
  Text: "متن",
  Image: "تصویر",
};

export const WRITER_PROMPT_CATEGORY_LABELS: Record<string, string> = {
  image: "تصویر",
  video: "ویدیو",
  coding: "کدنویسی",
  writing: "نوشتار",
  marketing: "بازاریابی",
  design: "طراحی",
  education: "آموزش",
};

export function labelForWriterPromptMediaType(value: string): string {
  return WRITER_PROMPT_MEDIA_TYPE_LABELS[value as WriterPromptMediaType] ?? value;
}

export function labelForWriterPromptCategory(name: string, slug?: string): string {
  const key = (slug ?? name).trim().toLowerCase();
  return WRITER_PROMPT_CATEGORY_LABELS[key] ?? name;
}

export function slugifyWriterPromptTitle(title: string): string {
  const slug = slugify(title).slice(0, WRITER_PROMPT_LIMITS.slug);
  return slug.length >= 2 ? slug : "prompt";
}

export function isKnownWriterPromptMediaType(value: string): value is WriterPromptMediaType {
  return (WRITER_PROMPT_MEDIA_TYPES as readonly string[]).includes(value);
}

export function parseWriterPromptTags(raw: string): string[] {
  return raw
    .split(/[,،]+/)
    .map((tag) => tag.trim())
    .filter((tag) => tag.length > 0);
}

export function validateWriterPromptForm(values: WriterPromptFormValues): WriterPromptFormErrors {
  const errors: WriterPromptFormErrors = {};
  const title = values.title.trim();
  const slug = values.slug.trim().toLowerCase();
  const content = values.content.trim();

  if (!title) {
    errors.title = "عنوان الزامی است.";
  } else if (title.length > WRITER_PROMPT_LIMITS.title) {
    errors.title = `عنوان حداکثر ${WRITER_PROMPT_LIMITS.title} نویسه است.`;
  }

  if (!slug) {
    errors.slug = "اسلاگ الزامی است.";
  } else if (!isValidSlug(slug) || slug.length > WRITER_PROMPT_LIMITS.slug) {
    errors.slug = "اسلاگ باید انگلیسی کوچک، عدد و خط تیره باشد.";
  }

  if (values.description.trim().length > WRITER_PROMPT_LIMITS.description) {
    errors.description = `توضیح حداکثر ${WRITER_PROMPT_LIMITS.description} نویسه است.`;
  }

  if (values.coverImage.trim().length > WRITER_PROMPT_LIMITS.coverImage) {
    errors.coverImage = "نشانی تصویر کاور خیلی بلند است.";
  }

  if (!content) {
    errors.content = "متن پرامپت الزامی است.";
  } else if (content.length > WRITER_PROMPT_LIMITS.content) {
    errors.content = `متن پرامپت حداکثر ${WRITER_PROMPT_LIMITS.content} نویسه است.`;
  }

  if (!values.aiModelId) {
    errors.aiModelId = "مدل هوش مصنوعی را انتخاب کنید.";
  }

  if (!values.categoryId) {
    errors.categoryId = "دسته‌بندی را انتخاب کنید.";
  }

  if (!isKnownWriterPromptMediaType(values.mediaType)) {
    errors.mediaType = "نوع رسانه نامعتبر است.";
  }

  return errors;
}

export function hasWriterPromptFormErrors(errors: WriterPromptFormErrors): boolean {
  return Object.values(errors).some(Boolean);
}
