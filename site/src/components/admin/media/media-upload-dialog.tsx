"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { MediaDropzone } from "@/components/admin/media/media-dropzone";
import { useUploadMediaAsset } from "@/lib/admin/media/media-hooks";
import { validateAltText, validateCaption, validateMediaFile } from "@/lib/admin/media/media-validation";
import { MEDIA_ALT_TEXT_MAX_LENGTH, MEDIA_CAPTION_MAX_LENGTH } from "@/lib/admin/media/media-types";
import type { AdminMediaDetail } from "@/lib/admin/media/media-types";

type MediaUploadDialogProps = {
  open: boolean;
  onClose: () => void;
  onUploaded: (detail: AdminMediaDetail) => void;
};

/**
 * Single-file upload modal (`POST /admin/media`, multipart/form-data).
 * There is no real byte-level progress signal from `fetch`, so this only ever
 * shows a boolean "در حال بارگذاری..." state — never a fabricated percentage.
 */
export function MediaUploadDialog({ open, onClose, onUploaded }: MediaUploadDialogProps) {
  const upload = useUploadMediaAsset();
  const [file, setFile] = useState<File | null>(null);
  const [altText, setAltText] = useState("");
  const [caption, setCaption] = useState("");
  const [fileError, setFileError] = useState<string | null>(null);
  const [altError, setAltError] = useState<string | null>(null);
  const [captionError, setCaptionError] = useState<string | null>(null);
  const dialogRef = useRef<HTMLDivElement>(null);

  const reset = useCallback(() => {
    setFile(null);
    setAltText("");
    setCaption("");
    setFileError(null);
    setAltError(null);
    setCaptionError(null);
    upload.reset();
  }, [upload]);

  const handleClose = useCallback(() => {
    if (upload.submitting) return;
    reset();
    onClose();
  }, [upload.submitting, reset, onClose]);

  useEffect(() => {
    if (!open) return;
    dialogRef.current?.querySelector<HTMLElement>("input,button")?.focus();
    const onKey = (event: KeyboardEvent) => {
      if (event.key === "Escape") handleClose();
    };
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [open, handleClose]);

  const handleFileSelected = useCallback((picked: File) => {
    setFile(picked);
    const result = validateMediaFile(picked);
    setFileError(result.valid ? null : result.error);
  }, []);

  const handleSubmit = useCallback(async () => {
    const fileResult = validateMediaFile(file);
    const altResult = validateAltText(altText);
    const captionResult = validateCaption(caption);

    setFileError(fileResult.valid ? null : fileResult.error);
    setAltError(altResult);
    setCaptionError(captionResult);

    if (!fileResult.valid || altResult || captionResult || !file) {
      return;
    }

    try {
      const detail = await upload.upload({
        file,
        altText: altText.trim() ? altText.trim() : null,
        caption: caption.trim() ? caption.trim() : null,
      });
      reset();
      onUploaded(detail);
      onClose();
    } catch {
      // Surfaced via upload.error below.
    }
  }, [file, altText, caption, upload, reset, onUploaded, onClose]);

  if (!open) return null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
      onMouseDown={handleClose}
    >
      <div
        ref={dialogRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby="media-upload-title"
        className="adm-surface w-full max-w-md space-y-4 rounded-xl p-5 shadow-[var(--adm-shadow)]"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <div className="flex items-center justify-between gap-2">
          <h3 id="media-upload-title" className="adm-text text-[15px] font-bold">
            بارگذاری رسانه
          </h3>
          <button
            type="button"
            onClick={handleClose}
            disabled={upload.submitting}
            aria-label="بستن"
            className="adm-btn adm-btn-ghost adm-focus p-1.5"
          >
            <AdminIcon name="close" size={16} />
          </button>
        </div>

        <MediaDropzone
          file={file}
          onFileSelected={handleFileSelected}
          error={fileError}
          disabled={upload.submitting}
        />

        <div className="space-y-1.5">
          <label htmlFor="media-alt-text" className="adm-text text-[12px] font-semibold">
            متن جایگزین (Alt)
          </label>
          <input
            id="media-alt-text"
            type="text"
            className="adm-input"
            value={altText}
            maxLength={MEDIA_ALT_TEXT_MAX_LENGTH}
            disabled={upload.submitting}
            onChange={(event) => setAltText(event.target.value)}
            aria-invalid={Boolean(altError)}
          />
          {altError ? (
            <p className="text-[11px] font-semibold text-[var(--adm-danger)]">{altError}</p>
          ) : null}
        </div>

        <div className="space-y-1.5">
          <label htmlFor="media-caption" className="adm-text text-[12px] font-semibold">
            عنوان تصویر (Caption)
          </label>
          <textarea
            id="media-caption"
            className="adm-input min-h-[64px] resize-y text-[13px] leading-6"
            value={caption}
            maxLength={MEDIA_CAPTION_MAX_LENGTH}
            disabled={upload.submitting}
            onChange={(event) => setCaption(event.target.value)}
            aria-invalid={Boolean(captionError)}
          />
          {captionError ? (
            <p className="text-[11px] font-semibold text-[var(--adm-danger)]">{captionError}</p>
          ) : null}
        </div>

        {upload.error ? <AdminErrorState error={upload.error} showHome={false} /> : null}

        <div className="flex justify-end gap-2">
          <button
            type="button"
            onClick={handleClose}
            disabled={upload.submitting}
            className="adm-btn adm-btn-outline adm-focus"
          >
            انصراف
          </button>
          <button
            type="button"
            onClick={() => void handleSubmit()}
            disabled={upload.submitting || !file}
            className="adm-btn adm-btn-primary adm-focus inline-flex items-center gap-1.5"
          >
            <AdminIcon name="check" size={16} />
            {upload.submitting ? "در حال بارگذاری..." : "بارگذاری"}
          </button>
        </div>
      </div>
    </div>
  );
}
