import {
  ACCEPTED_MEDIA_CONTENT_TYPES,
  MAX_MEDIA_UPLOAD_SIZE_BYTES,
  MEDIA_ALT_TEXT_MAX_LENGTH,
  MEDIA_CAPTION_MAX_LENGTH,
} from "@/lib/admin/media/media-types";

/**
 * Client-side upload UX guards only — the server remains authoritative.
 * Rejects: no file, empty file, oversized file, SVG (script-injection risk),
 * and any MIME type outside the JPEG/PNG/WebP allow-list.
 */

const SVG_CONTENT_TYPE = "image/svg+xml";

export type MediaFileValidationResult =
  | { valid: true }
  | { valid: false; error: string };

export function validateMediaFile(file: File | null | undefined): MediaFileValidationResult {
  if (!file) {
    return { valid: false, error: "فایلی انتخاب نشده است." };
  }

  if (file.size <= 0) {
    return { valid: false, error: "فایل انتخاب‌شده خالی است." };
  }

  if (file.size > MAX_MEDIA_UPLOAD_SIZE_BYTES) {
    return { valid: false, error: "حجم فایل نباید بیش از ۵ مگابایت باشد." };
  }

  const type = (file.type || "").toLowerCase();
  const looksLikeSvg = type === SVG_CONTENT_TYPE || file.name.toLowerCase().endsWith(".svg");
  if (looksLikeSvg) {
    return { valid: false, error: "فایل‌های SVG پذیرفته نمی‌شوند." };
  }

  const isAccepted = (ACCEPTED_MEDIA_CONTENT_TYPES as readonly string[]).includes(type);
  if (!isAccepted) {
    return { valid: false, error: "فقط تصاویر JPEG، PNG و WebP پذیرفته می‌شوند." };
  }

  return { valid: true };
}

export function validateAltText(value: string): string | null {
  if (value.trim().length > MEDIA_ALT_TEXT_MAX_LENGTH) {
    return `متن جایگزین نباید بیش از ${MEDIA_ALT_TEXT_MAX_LENGTH} نویسه باشد.`;
  }
  return null;
}

export function validateCaption(value: string): string | null {
  if (value.trim().length > MEDIA_CAPTION_MAX_LENGTH) {
    return `عنوان تصویر نباید بیش از ${MEDIA_CAPTION_MAX_LENGTH} نویسه باشد.`;
  }
  return null;
}
