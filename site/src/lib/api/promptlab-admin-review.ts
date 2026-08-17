import { apiRequest } from "./client";

export type AdminPromptReviewStatusDto = "Submitted" | "Approved" | "Rejected";

export type AdminPromptReviewListItemDto = {
  id: string;
  title: string;
  slug: string;
  authorId: string;
  categoryId: string;
  categoryName: string;
  preview: string;
  status: AdminPromptReviewStatusDto;
  rejectionReason: string | null;
  createdAt: string;
  updatedAt: string;
  publishedAt: string | null;
};

export type AdminPromptReviewPageDto = {
  page: number;
  pageSize: number;
  total: number;
  items: AdminPromptReviewListItemDto[];
};

export type AdminPromptReviewFilter = {
  status: AdminPromptReviewStatusDto;
  page?: number;
  pageSize?: number;
};

export function listAdminReviewPrompts(
  token: string,
  filter: AdminPromptReviewFilter,
  signal?: AbortSignal,
): Promise<AdminPromptReviewPageDto> {
  return apiRequest<AdminPromptReviewPageDto>({
    path: "/admin/prompts",
    token,
    query: filter,
    signal,
  });
}

export function approveAdminPrompt(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<AdminPromptReviewListItemDto> {
  return apiRequest<AdminPromptReviewListItemDto>({
    method: "POST",
    path: `/admin/prompts/${encodeURIComponent(id)}/approve`,
    token,
    signal,
  });
}

export function rejectAdminPrompt(
  token: string,
  id: string,
  reason: string,
  signal?: AbortSignal,
): Promise<AdminPromptReviewListItemDto> {
  return apiRequest<AdminPromptReviewListItemDto>({
    method: "POST",
    path: `/admin/prompts/${encodeURIComponent(id)}/reject`,
    token,
    body: { reason },
    signal,
  });
}
