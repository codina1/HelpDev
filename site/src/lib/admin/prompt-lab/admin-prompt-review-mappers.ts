import type { AdminPromptReviewListItemDto, AdminPromptReviewPageDto } from "@/lib/api/promptlab-admin-review";
import { labelForWriterPromptStatus } from "@/lib/admin/prompt-lab/writer-prompt-mappers";
import type { WriterPromptStatus } from "@/lib/admin/prompt-lab/writer-prompt-types";
import type { AdminPromptReviewItem, AdminPromptReviewPage } from "./admin-prompt-review-types";

function toStatus(status: string): AdminPromptReviewItem["status"] {
  if (status === "Approved" || status === "Rejected" || status === "Submitted") {
    return status;
  }
  return "Submitted";
}

export function mapAdminPromptReviewItem(raw: AdminPromptReviewListItemDto): AdminPromptReviewItem {
  const status = toStatus(raw.status);
  return {
    id: raw.id,
    title: raw.title,
    slug: raw.slug,
    authorId: raw.authorId,
    categoryName: raw.categoryName,
    preview: raw.preview,
    status,
    rejectionReason: raw.rejectionReason,
  };
}

export function mapAdminPromptReviewPage(raw: AdminPromptReviewPageDto): AdminPromptReviewPage {
  const totalPages = raw.pageSize > 0 ? Math.max(1, Math.ceil(raw.total / raw.pageSize)) : 1;
  return {
    page: raw.page,
    pageSize: raw.pageSize,
    totalCount: raw.total,
    totalPages: raw.total === 0 ? 0 : totalPages,
    items: raw.items.map(mapAdminPromptReviewItem),
  };
}

export function labelForAdminPromptReviewStatus(status: WriterPromptStatus): string {
  return labelForWriterPromptStatus(status);
}
