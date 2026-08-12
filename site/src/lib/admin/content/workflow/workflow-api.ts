import {
  approveContent as approveContentRequest,
  archiveContent as archiveContentRequest,
  getContentWorkflowHistory as getContentWorkflowHistoryRequest,
  publishContent as publishContentRequest,
  rejectContent as rejectContentRequest,
  submitContentForReview as submitContentForReviewRequest,
} from "@/lib/api/content";
import type { AdminContentDetailRawDto } from "@/lib/admin/content/content-types";
import type {
  RejectContentPayload,
  WorkflowHistoryRawDto,
} from "@/lib/admin/content/workflow/workflow-types";

/**
 * Content workflow data access. Reuses `@/lib/api/content` (no duplicated fetch).
 */

export const CONTENT_WORKFLOW_CAPABILITIES = {
  submitReview: true,
  approve: true,
  reject: true,
  publish: true,
  archive: true,
  history: true,
} as const;

/** Raised when workflow operations require auth or are unavailable. */
export class ContentWorkflowOperationError extends Error {
  constructor(message = "این عملیات گردش کار هنوز توسط سرور پشتیبانی نمی‌شود.") {
    super(message);
    this.name = "ContentWorkflowOperationError";
  }
}

export async function fetchContentWorkflowHistory(
  token: string,
  contentId: string,
  signal?: AbortSignal,
): Promise<WorkflowHistoryRawDto> {
  return getContentWorkflowHistoryRequest(token, contentId, signal);
}

export async function submitContentForReviewItem(
  token: string,
  contentId: string,
  signal?: AbortSignal,
): Promise<AdminContentDetailRawDto> {
  const detail = await submitContentForReviewRequest(token, contentId, signal);
  return detail as unknown as AdminContentDetailRawDto;
}

export async function approveContentItem(
  token: string,
  contentId: string,
  signal?: AbortSignal,
): Promise<AdminContentDetailRawDto> {
  const detail = await approveContentRequest(token, contentId, signal);
  return detail as unknown as AdminContentDetailRawDto;
}

export async function rejectContentItem(
  token: string,
  contentId: string,
  payload: RejectContentPayload,
  signal?: AbortSignal,
): Promise<AdminContentDetailRawDto> {
  const detail = await rejectContentRequest(token, contentId, payload, signal);
  return detail as unknown as AdminContentDetailRawDto;
}

export async function publishContentWorkflowItem(
  token: string,
  contentId: string,
  signal?: AbortSignal,
): Promise<AdminContentDetailRawDto> {
  const detail = await publishContentRequest(token, contentId, signal);
  return detail as unknown as AdminContentDetailRawDto;
}

export async function archiveContentItem(
  token: string,
  contentId: string,
  signal?: AbortSignal,
): Promise<AdminContentDetailRawDto> {
  const detail = await archiveContentRequest(token, contentId, signal);
  return detail as unknown as AdminContentDetailRawDto;
}
