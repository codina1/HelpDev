import { API_BASE_URL } from "@/lib/config";
import type {
  AdminMediaDetail,
  AdminMediaListItem,
  AdminMediaPagedResult,
  MediaAssetListItemRawDto,
  MediaAssetRawDto,
  MediaPagedResultRawDto,
  MediaPickerSelection,
} from "@/lib/admin/media/media-types";

/**
 * Resolves a relative media URL (e.g. `/media/2026/07/{guid}.jpg`) against the
 * API **origin** — not the versioned `/api/v1` base — since static media files
 * are served from the API host root, not under the API prefix. Already-absolute
 * URLs (and data URLs, defensively) are returned unchanged.
 */
export function resolveMediaUrl(publicUrl: string): string {
  if (!publicUrl) return "";
  if (/^https?:\/\//i.test(publicUrl) || publicUrl.startsWith("data:")) {
    return publicUrl;
  }
  try {
    const origin = new URL(API_BASE_URL).origin;
    return new URL(publicUrl, origin).toString();
  } catch {
    return publicUrl;
  }
}

const BYTE_UNITS = ["بایت", "کیلوبایت", "مگابایت", "گیگابایت"] as const;
const NUMBER_FORMAT_FA = new Intl.NumberFormat("fa-IR");

/** Human-readable file size in Persian units (no raw byte-precision noise). */
export function formatFileSize(bytes: number): string {
  if (!Number.isFinite(bytes) || bytes < 0) return "—";
  if (bytes < 1024) return `${NUMBER_FORMAT_FA.format(bytes)} ${BYTE_UNITS[0]}`;

  let value = bytes / 1024;
  let unitIndex = 1;
  while (value >= 1024 && unitIndex < BYTE_UNITS.length - 1) {
    value /= 1024;
    unitIndex += 1;
  }
  const rounded = value >= 10 ? Math.round(value) : Math.round(value * 10) / 10;
  return `${NUMBER_FORMAT_FA.format(rounded)} ${BYTE_UNITS[unitIndex]}`;
}

const CONTENT_TYPE_LABELS: Record<string, string> = {
  "image/jpeg": "JPEG",
  "image/png": "PNG",
  "image/webp": "WebP",
};

export function labelForMediaContentType(contentType: string): string {
  return CONTENT_TYPE_LABELS[contentType.toLowerCase()] ?? contentType;
}

const DATE_FORMAT_FA = new Intl.DateTimeFormat("fa-IR", {
  year: "numeric",
  month: "long",
  day: "numeric",
});

export function formatDateFa(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return "";
  return DATE_FORMAT_FA.format(date);
}

export function mapAdminMediaListItem(dto: MediaAssetListItemRawDto): AdminMediaListItem {
  const publicUrl = dto.publicUrl ?? "";
  return {
    id: dto.id,
    originalFileName: dto.originalFileName,
    contentType: dto.contentType,
    sizeBytes: dto.sizeBytes,
    width: dto.width,
    height: dto.height,
    publicUrl,
    absoluteUrl: resolveMediaUrl(publicUrl),
    altText: dto.altText ?? "",
    uploadedByUserId: dto.uploadedByUserId,
    createdAtUtc: dto.createdAtUtc,
    status: dto.status,
  };
}

export function mapAdminMediaPagedResult(dto: MediaPagedResultRawDto): AdminMediaPagedResult {
  const pageSize = dto.pageSize > 0 ? dto.pageSize : 1;
  const totalPages =
    typeof dto.totalPages === "number" && dto.totalPages >= 0
      ? dto.totalPages
      : Math.ceil((dto.totalCount ?? 0) / pageSize);

  return {
    items: (dto.items ?? []).map(mapAdminMediaListItem),
    page: dto.page ?? 1,
    pageSize: dto.pageSize,
    totalCount: dto.totalCount ?? 0,
    totalPages,
  };
}

/** Maps the full detail DTO (upload response or GET /admin/media/{id}). */
export function mapAdminMediaDetail(dto: MediaAssetRawDto): AdminMediaDetail {
  const item = mapAdminMediaListItem(dto);
  return {
    ...item,
    caption: dto.caption ?? "",
    updatedAtUtc: dto.updatedAtUtc,
  };
}

/** Builds the picker selection payload handed to Cover image / OG image consumers. */
export function toMediaPickerSelection(item: AdminMediaListItem): MediaPickerSelection {
  return {
    id: item.id,
    publicUrl: item.publicUrl,
    absoluteUrl: item.absoluteUrl,
    altText: item.altText,
    width: item.width,
    height: item.height,
  };
}
