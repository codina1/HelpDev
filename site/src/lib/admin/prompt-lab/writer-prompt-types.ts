import { ADMIN_CONTENT_PAGE_SIZE_DEFAULT, ADMIN_CONTENT_PAGE_SIZES } from "@/lib/admin/content/content-types";

export const WRITER_PROMPT_STATUSES = ["Draft", "Submitted", "Approved", "Rejected"] as const;

export type WriterPromptStatus = (typeof WRITER_PROMPT_STATUSES)[number];

export type WriterPromptListItem = {
  id: string;
  title: string;
  slug: string;
  status: WriterPromptStatus;
  statusLabel: string;
  views: number;
  copyCount: number;
  createdAt: string;
};

export type WriterPromptPagedResult = {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  items: WriterPromptListItem[];
};

export type WriterPromptStats = {
  total: number;
  drafts: number;
  pendingReview: number;
  published: number;
};

export const WRITER_PROMPT_PAGE_SIZES = ADMIN_CONTENT_PAGE_SIZES;
export type WriterPromptPageSize = (typeof WRITER_PROMPT_PAGE_SIZES)[number];
export const WRITER_PROMPT_PAGE_SIZE_DEFAULT = ADMIN_CONTENT_PAGE_SIZE_DEFAULT;

export type WriterPromptListQuery = {
  page: number;
  pageSize: WriterPromptPageSize;
  status: WriterPromptStatus | "all";
};

export const DEFAULT_WRITER_PROMPT_LIST_QUERY: WriterPromptListQuery = {
  page: 1,
  pageSize: WRITER_PROMPT_PAGE_SIZE_DEFAULT,
  status: "all",
};
