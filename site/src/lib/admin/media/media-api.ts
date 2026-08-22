import {
  deleteMediaAsset as deleteMediaAssetRequest,
  getAdminMediaById as getAdminMediaByIdRequest,
  getAdminMediaConfig as getAdminMediaConfigRequest,
  getAdminMediaList as getAdminMediaListRequest,
  updateMediaAsset as updateMediaAssetRequest,
  uploadMediaAsset as uploadMediaAssetRequest,
  type AdminMediaListOptions,
} from "@/lib/api/media";
import type {
  AdminMediaListQuery,
  MediaAssetRawDto,
  MediaLibraryConfigRawDto,
  MediaPagedResultRawDto,
  UpdateMediaPayload,
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
  config: true,
  update: true,
  delete: true,
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

/** GET /admin/media/config — upload limits. */
export async function fetchAdminMediaConfig(
  token: string,
  signal?: AbortSignal,
): Promise<MediaLibraryConfigRawDto> {
  return getAdminMediaConfigRequest(token, signal);
}

/** PUT /admin/media/{id} — alt/caption. */
export async function updateMediaAssetItem(
  token: string,
  id: string,
  payload: UpdateMediaPayload,
  signal?: AbortSignal,
): Promise<MediaAssetRawDto> {
  const detail = await updateMediaAssetRequest(
    token,
    id,
    { altText: payload.altText, caption: payload.caption },
    signal,
  );
  return detail as unknown as MediaAssetRawDto;
}

/** DELETE /admin/media/{id} — archive + storage cleanup. */
export async function deleteMediaAssetItem(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<void> {
  await deleteMediaAssetRequest(token, id, signal);
}
