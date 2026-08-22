/**
 * Media Library data contracts.
 *
 * Mirrors the real backend Media Management API only:
 * - `POST /admin/media` (multipart/form-data) — upload a single image.
 * - `GET /admin/media` — paged, searchable list (`MediaAssetListItemDto`).
 * - `GET /admin/media/{id}` — full detail (`MediaAssetDto`).
 * - `GET /admin/media/config` — upload limits.
 * - `PUT /admin/media/{id}` — update alt/caption.
 * - `DELETE /admin/media/{id}` — archive and remove stored bytes (UI must confirm).
 */

export const ADMIN_MEDIA_PAGE_SIZE_DEFAULT = 24;
export const ADMIN_MEDIA_PAGE_SIZE_MAX = 100;
export const ADMIN_MEDIA_PAGE_SIZES = [12, 24, 48, 96] as const;
export type AdminMediaPageSize = (typeof ADMIN_MEDIA_PAGE_SIZES)[number];

/**
 * Client-side upload allow-list (UX guard only — server remains authoritative).
 * SVG is explicitly excluded: it can embed scripts and must never be accepted.
 */
export const ACCEPTED_MEDIA_CONTENT_TYPES = ["image/jpeg", "image/png", "image/webp"] as const;
export type AcceptedMediaContentType = (typeof ACCEPTED_MEDIA_CONTENT_TYPES)[number];

/** Client-side UX guard mirroring the expected server limit. */
export const MAX_MEDIA_UPLOAD_SIZE_BYTES = 5 * 1024 * 1024;

export const MEDIA_ALT_TEXT_MAX_LENGTH = 300;
export const MEDIA_CAPTION_MAX_LENGTH = 500;

/* ------------------------------- Raw API DTOs ------------------------------ */

// MediaAssetDto — full detail (upload response + GET /admin/media/{id}).
export type MediaAssetRawDto = {
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
export type MediaAssetListItemRawDto = {
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

/** Raw PagedResult&lt;MediaAssetListItemDto&gt;. */
export type MediaPagedResultRawDto = {
  items: MediaAssetListItemRawDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

/* ------------------------ List query / view models -------------------------- */

/** Normalized workspace query (URL-driven: page, pageSize, search). */
export type AdminMediaListQuery = {
  page: number;
  pageSize: AdminMediaPageSize;
  search: string;
};

export const DEFAULT_ADMIN_MEDIA_LIST_QUERY: AdminMediaListQuery = {
  page: 1,
  pageSize: ADMIN_MEDIA_PAGE_SIZE_DEFAULT,
  search: "",
};

/** View model for a single grid item — never exposes a storage key/path. */
export type AdminMediaListItem = {
  id: string;
  originalFileName: string;
  contentType: string;
  sizeBytes: number;
  width: number | null;
  height: number | null;
  /** Raw relative URL as returned by the server (e.g. `/media/2026/07/{guid}.jpg`). */
  publicUrl: string;
  /** `publicUrl` resolved against the API origin — safe to use directly in `<img src>`. */
  absoluteUrl: string;
  altText: string;
  uploadedByUserId: string;
  createdAtUtc: string;
  status: string;
};

export type AdminMediaPagedResult = {
  items: AdminMediaListItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

/** Full detail view model (adds caption + updatedAtUtc over the list item). */
export type AdminMediaDetail = AdminMediaListItem & {
  caption: string;
  updatedAtUtc: string;
};

/* --------------------------------- Upload ----------------------------------- */

export type UploadMediaPayload = {
  file: File;
  altText: string | null;
  caption: string | null;
};

export type UpdateMediaPayload = {
  altText: string | null;
  caption: string | null;
};

export type MediaLibraryConfigRawDto = {
  maxUploadBytes: number;
  maxWidth: number;
  maxHeight: number;
  allowedContentTypes: string[];
  maxAltTextLength: number;
  maxCaptionLength: number;
};

/**
 * Result handed back to any picker consumer (Cover image / OG image fields).
 * Intentionally excludes storage keys / filesystem paths.
 */
export type MediaPickerSelection = {
  id: string;
  publicUrl: string;
  /** `publicUrl` resolved against the API origin — the value pickers should store in URL fields. */
  absoluteUrl: string;
  altText: string;
  width: number | null;
  height: number | null;
};
