import {
  getContentRevision as getContentRevisionRequest,
  getContentRevisions as getContentRevisionsRequest,
  restoreContentRevision as restoreContentRevisionRequest,
  type ContentRevisionListOptions,
} from "@/lib/api/content";
import type { AdminContentDetailRawDto } from "@/lib/admin/content/content-types";
import type {
  ContentRevisionDetailRawDto,
  ContentRevisionListQuery,
  ContentRevisionPagedResultRawDto,
  RestoreContentRevisionPayload,
} from "@/lib/admin/content/history/history-types";

/**
 * Content revision history data access. Reuses `@/lib/api/content` (no duplicated fetch).
 */

export const CONTENT_REVISION_CAPABILITIES = {
  list: true,
  detail: true,
  restore: true,
} as const;

/** Raised when revision operations require auth or are unavailable. */
export class ContentRevisionOperationError extends Error {
  constructor(message = "این عملیات هنوز توسط سرور پشتیبانی نمی‌شود.") {
    super(message);
    this.name = "ContentRevisionOperationError";
  }
}

export function toContentRevisionListOptions(query: ContentRevisionListQuery): ContentRevisionListOptions {
  return {
    page: query.page,
    pageSize: query.pageSize,
  };
}

export function contentRevisionListQueryKey(contentId: string, query: ContentRevisionListQuery): string {
  return `${contentId}|${query.page}|${query.pageSize}`;
}

// GET /admin/content/{id}/revisions
export async function fetchContentRevisions(
  token: string,
  contentId: string,
  query: ContentRevisionListQuery,
  signal?: AbortSignal,
): Promise<ContentRevisionPagedResultRawDto> {
  const result = await getContentRevisionsRequest(
    token,
    contentId,
    toContentRevisionListOptions(query),
    signal,
  );
  return result as unknown as ContentRevisionPagedResultRawDto;
}

// GET /admin/content/{id}/revisions/{version}
export async function fetchContentRevisionDetail(
  token: string,
  contentId: string,
  version: number,
  signal?: AbortSignal,
): Promise<ContentRevisionDetailRawDto> {
  const detail = await getContentRevisionRequest(token, contentId, version, signal);
  return detail as unknown as ContentRevisionDetailRawDto;
}

// POST /admin/content/{id}/revisions/{version}/restore
export async function restoreContentRevisionItem(
  token: string,
  contentId: string,
  version: number,
  payload: RestoreContentRevisionPayload = {},
  signal?: AbortSignal,
): Promise<AdminContentDetailRawDto> {
  const detail = await restoreContentRevisionRequest(
    token,
    contentId,
    version,
    payload.changeReason != null ? { changeReason: payload.changeReason } : {},
    signal,
  );
  return detail as unknown as AdminContentDetailRawDto;
}
