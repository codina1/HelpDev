import type { WriterPromptListItemDto, WriterPromptPageDto } from "@/lib/api/promptlab-writer";
import type {
  WriterPromptListItem,
  WriterPromptPagedResult,
  WriterPromptStatus,
} from "./writer-prompt-types";
import { WRITER_PROMPT_STATUSES } from "./writer-prompt-types";

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
