import { apiRequest } from "./client";

/**
 * Media Library resource — real backend contract only.
 *
 * - `POST /admin/media` (multipart/form-data): uploads a single image and
 *   returns the full `MediaAssetDto`.
 * - `GET /admin/media`: server-paginated list (`PagedResult<MediaAssetListItemDto>`).
 * - `GET /admin/media/{id}`: full `MediaAssetDto` detail.
 *
 * There is NO delete endpoint — do not add one here or in the UI.
 */

// MediaAssetDto — full detail (upload response + GET /admin/media/{id}).
export type MediaAssetDto = {
  id: string;
  originalFileName: string;
  contentType: string;
  sizeBytes: number;
  width: number | null;
  height: number | null;
  publicUrl: string;
  altText: string | null;
  caption: string | null;
  uploadedByUserId: string;
  createdAtUtc: string;
  updatedAtUtc: string;
  status: string;
};

// MediaAssetListItemDto — list projection (no caption/updatedAtUtc).
export type MediaAssetListItemDto = {
  id: string;
  originalFileName: string;
  contentType: string;
  sizeBytes: number;
  width: number | null;
  height: number | null;
  publicUrl: string;
  altText: string | null;
  uploadedByUserId: string;
  createdAtUtc: string;
  status: string;
};

/** PagedResult&lt;MediaAssetListItemDto&gt; (camelCase JSON). */
export type MediaPagedResultDto = {
  items: MediaAssetListItemDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type AdminMediaListOptions = {
  page?: number;
  pageSize?: number;
  search?: string;
  contentType?: string;
};

export type UploadMediaAssetRequest = {
  file: File;
  altText?: string | null;
  caption?: string | null;
};

/**
 * POST /admin/media (multipart/form-data). The shared `apiRequest` client
 * detects the `FormData` body and lets the browser set its own
 * `Content-Type` (with boundary) — never set it manually here.
 */
export function uploadMediaAsset(
  token: string,
  request: UploadMediaAssetRequest,
  signal?: AbortSignal,
): Promise<MediaAssetDto> {
  const formData = new FormData();
  formData.append("file", request.file);
  if (request.altText) formData.append("altText", request.altText);
  if (request.caption) formData.append("caption", request.caption);

  return apiRequest<MediaAssetDto>({
    method: "POST",
    path: "/admin/media",
    token,
    body: formData,
    signal,
  });
}

/** GET /admin/media — server-side pagination + optional search/contentType filters. */
export function getAdminMediaList(
  token: string,
  options: AdminMediaListOptions = {},
  signal?: AbortSignal,
): Promise<MediaPagedResultDto> {
  const query: Record<string, string | number | undefined> = {};

  if (options.page != null) query.page = options.page;
  if (options.pageSize != null) query.pageSize = options.pageSize;
  if (options.search) query.search = options.search;
  if (options.contentType) query.contentType = options.contentType;

  return apiRequest<MediaPagedResultDto>({
    path: "/admin/media",
    token,
    query,
    signal,
  });
}

/** GET /admin/media/{id} — full detail. */
export function getAdminMediaById(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<MediaAssetDto> {
  return apiRequest<MediaAssetDto>({
    path: `/admin/media/${encodeURIComponent(id)}`,
    token,
    signal,
  });
}
