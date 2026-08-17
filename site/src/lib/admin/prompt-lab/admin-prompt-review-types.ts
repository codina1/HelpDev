import { ADMIN_CONTENT_PAGE_SIZE_DEFAULT, ADMIN_CONTENT_PAGE_SIZES } from "@/lib/admin/content/content-types";
import type { WriterPromptStatus } from "@/lib/admin/prompt-lab/writer-prompt-types";

export const ADMIN_PROMPT_REVIEW_TABS = ["pending", "published", "rejected"] as const;
export type AdminPromptReviewTab = (typeof ADMIN_PROMPT_REVIEW_TABS)[number];

export const ADMIN_PROMPT_REVIEW_TAB_STATUS: Record<AdminPromptReviewTab, "Submitted" | "Approved" | "Rejected"> = {
  pending: "Submitted",
  published: "Approved",
  rejected: "Rejected",
};

export const ADMIN_PROMPT_REVIEW_TAB_LABELS: Record<AdminPromptReviewTab, string> = {
  pending: "در انتظار",
  published: "منتشرشده",
  rejected: "ردشده",
};

export type AdminPromptReviewItem = {
  id: string;
  title: string;
  slug: string;
  authorId: string;
  categoryName: string;
  preview: string;
  status: Extract<WriterPromptStatus, "Submitted" | "Approved" | "Rejected">;
  rejectionReason: string | null;
};

export type AdminPromptReviewPage = {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  items: AdminPromptReviewItem[];
};

export const ADMIN_PROMPT_REVIEW_PAGE_SIZES = ADMIN_CONTENT_PAGE_SIZES;
export type AdminPromptReviewPageSize = (typeof ADMIN_PROMPT_REVIEW_PAGE_SIZES)[number];

export type AdminPromptReviewQuery = {
  tab: AdminPromptReviewTab;
  page: number;
  pageSize: AdminPromptReviewPageSize;
};

export const DEFAULT_ADMIN_PROMPT_REVIEW_QUERY: AdminPromptReviewQuery = {
  tab: "pending",
  page: 1,
  pageSize: ADMIN_CONTENT_PAGE_SIZE_DEFAULT,
};
