import {
  getAdminMediaById as getAdminMediaByIdRequest,
  getAdminMediaList as getAdminMediaListRequest,
  uploadMediaAsset as uploadMediaAssetRequest,
  type AdminMediaListOptions,
} from "@/lib/api/media";
import type {
  AdminMediaListQuery,
  MediaAssetRawDto,
  MediaPagedResultRawDto,
  UploadMediaPayload,
} from "@/lib/admin/media/media-types";

/**
 * Media Library data access. Reuses the shared typed API client via the
 * existing `@/lib/api/media` module (no duplicated fetch logic) and targets
 * only canonical `/api/v1` routes.
 */

export const MEDIA_CAPABILITIES = {
  upload: true,
  list: true,
  getById: true,
  // The backend has NO delete endpoint — never flip this to true without one.
  delete: false,
} as const;

/** Raised when a not-yet-supported (or unauthenticated) media operation is invoked. */
export class MediaOperationUnsupportedError extends Error {
  constructor(message = "این عملیات هنوز توسط سرور پشتیبانی نمی‌شود.") {
    super(message);
    this.name = "MediaOperationUnsupportedError";
  }
}

/** Maps the workspace query to API options (omits empty filters). */
export function toAdminMediaListOptions(query: AdminMediaListQuery): AdminMediaListOptions {
  return {
    page: query.page,
    pageSize: query.pageSize,
    search: query.search.trim() || undefined,
  };
}

// GET /admin/media — paged, searchable list.
export async function fetchAdminMediaList(
  token: string,
  query: AdminMediaListQuery,
  signal?: AbortSignal,
): Promise<MediaPagedResultRawDto> {
  const result = await getAdminMediaListRequest(token, toAdminMediaListOptions(query), signal);
  return result as unknown as MediaPagedResultRawDto;
}

// GET /admin/media/{id} — full detail.
export async function fetchAdminMediaById(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<MediaAssetRawDto> {
  const detail = await getAdminMediaByIdRequest(token, id, signal);
  return detail as unknown as MediaAssetRawDto;
}

// POST /admin/media (multipart/form-data) — single-file upload.
export async function uploadMediaAssetItem(
  token: string,
  payload: UploadMediaPayload,
  signal?: AbortSignal,
): Promise<MediaAssetRawDto> {
  const detail = await uploadMediaAssetRequest(
    token,
    { file: payload.file, altText: payload.altText, caption: payload.caption },
    signal,
  );
  return detail as unknown as MediaAssetRawDto;
}
