"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { ACCEPTED_MEDIA_CONTENT_TYPES } from "@/lib/admin/media/media-types";

const ACCEPT_ATTR = ACCEPTED_MEDIA_CONTENT_TYPES.join(",");

type MediaDropzoneProps = {
  file: File | null;
  onFileSelected: (file: File) => void;
  error?: string | null;
  disabled?: boolean;
};

/**
 * Single-file drag-and-drop + click-to-browse zone. Local preview uses a
 * short-lived `URL.createObjectURL` blob that is revoked as soon as the
 * selected file changes or the component unmounts — never persisted anywhere.
 */
export function MediaDropzone({ file, onFileSelected, error, disabled = false }: MediaDropzoneProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [isDragging, setIsDragging] = useState(false);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);

  useEffect(() => {
    if (!file) {
      setPreviewUrl(null);
      return;
    }
    const url = URL.createObjectURL(file);
    setPreviewUrl(url);
    return () => URL.revokeObjectURL(url);
  }, [file]);

  const pick = useCallback(
    (picked: File | null | undefined) => {
      if (!picked) return;
      onFileSelected(picked);
    },
    [onFileSelected],
  );

  const onDrop = useCallback(
    (event: React.DragEvent<HTMLDivElement>) => {
      event.preventDefault();
      setIsDragging(false);
      if (disabled) return;
      pick(event.dataTransfer.files?.[0]);
    },
    [disabled, pick],
  );

  return (
    <div className="space-y-2">
      <div
        role="button"
        tabIndex={disabled ? -1 : 0}
        aria-disabled={disabled}
        onClick={() => !disabled && inputRef.current?.click()}
        onKeyDown={(event) => {
          if (disabled) return;
          if (event.key === "Enter" || event.key === " ") {
            event.preventDefault();
            inputRef.current?.click();
          }
        }}
        onDragOver={(event) => {
          event.preventDefault();
          if (!disabled) setIsDragging(true);
        }}
        onDragLeave={() => setIsDragging(false)}
        onDrop={onDrop}
        className={`adm-focus flex min-h-[160px] flex-col items-center justify-center gap-2 rounded-xl border-2 border-dashed p-6 text-center transition-colors ${
          isDragging
            ? "border-[var(--adm-accent)] bg-[var(--adm-accent-soft)]"
            : "border-[var(--adm-border)]"
        } ${disabled ? "cursor-not-allowed opacity-60" : "cursor-pointer"}`}
      >
        {previewUrl ? (
          // eslint-disable-next-line @next/next/no-img-element
          <img src={previewUrl} alt="" className="h-24 w-24 rounded-lg object-cover" />
        ) : (
          <AdminIcon name="media" size={32} className="adm-subtle" />
        )}
        <p className="adm-text text-[13px] font-semibold">
          {file ? file.name : "برای انتخاب فایل کلیک کنید یا آن را اینجا رها کنید"}
        </p>
        <p className="adm-subtle text-[11px]">JPEG، PNG یا WebP — حداکثر ۵ مگابایت</p>
        <input
          ref={inputRef}
          type="file"
          accept={ACCEPT_ATTR}
          className="sr-only"
          disabled={disabled}
          onChange={(event) => pick(event.target.files?.[0])}
        />
      </div>
      {error ? <p className="text-[11px] font-semibold text-[var(--adm-danger)]">{error}</p> : null}
    </div>
  );
}
