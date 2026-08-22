"use client";

import { useEffect, useState } from "react";
import styles from "./article-rich-text-editor.module.css";

export type ImageInsertValue = {
  src: string;
  alt: string;
  title: string;
  caption: string;
};

type ImageInsertDialogProps = {
  open: boolean;
  onClose: () => void;
  onInsertUrl: (value: ImageInsertValue) => void;
  onPickFile: (file: File) => void;
  onOpenLibrary: () => void;
};

export function ImageInsertDialog({
  open,
  onClose,
  onInsertUrl,
  onPickFile,
  onOpenLibrary,
}: ImageInsertDialogProps) {
  const [value, setValue] = useState<ImageInsertValue>({ src: "", alt: "", title: "", caption: "" });
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (open) {
      setValue({ src: "", alt: "", title: "", caption: "" });
      setError(null);
    }
  }, [open]);

  if (!open) return null;

  return (
    <div className={styles.dialogBackdrop} onMouseDown={onClose}>
      <form
        className={styles.dialog}
        dir="rtl"
        onMouseDown={(event) => event.stopPropagation()}
        onSubmit={(event) => {
          event.preventDefault();
          const src = value.src.trim();
          if (!isSafeImageSrc(src)) {
            setError("فقط نشانی http(s) یا مسیر داخلی مجاز است. تصویر Base64 ذخیره نمی‌شود.");
            return;
          }
          onInsertUrl({ ...value, src });
        }}
      >
        <h2 className="adm-text mb-3 text-[15px] font-bold">درج تصویر میان متن</h2>
        <label className="mb-2 block space-y-1">
          <span className="adm-text text-[12px] font-semibold">انتخاب فایل</span>
          <input
            className="adm-input"
            type="file"
            accept="image/jpeg,image/png,image/webp,image/gif,.jpg,.jpeg,.png,.webp"
            onChange={(event) => {
              const file = event.target.files?.[0];
              if (file) onPickFile(file);
            }}
          />
        </label>
        <label className="mb-2 block space-y-1">
          <span className="adm-text text-[12px] font-semibold">یا نشانی تصویر</span>
          <input
            className="adm-input text-start"
            dir="ltr"
            value={value.src}
            placeholder="https://… یا /media/…"
            onChange={(event) => setValue((prev) => ({ ...prev, src: event.target.value }))}
          />
        </label>
        <label className="mb-2 block space-y-1">
          <span className="adm-text text-[12px] font-semibold">متن جایگزین</span>
          <input className="adm-input" value={value.alt} onChange={(event) => setValue((prev) => ({ ...prev, alt: event.target.value }))} />
        </label>
        <label className="mb-2 block space-y-1">
          <span className="adm-text text-[12px] font-semibold">عنوان</span>
          <input className="adm-input" value={value.title} onChange={(event) => setValue((prev) => ({ ...prev, title: event.target.value }))} />
        </label>
        <label className="mb-3 block space-y-1">
          <span className="adm-text text-[12px] font-semibold">شرح تصویر</span>
          <input className="adm-input" value={value.caption} onChange={(event) => setValue((prev) => ({ ...prev, caption: event.target.value }))} />
        </label>
        {error ? <p className={styles.errorText}>{error}</p> : null}
        <div className="flex flex-wrap justify-end gap-2">
          <button type="button" className="adm-btn adm-btn-ghost adm-focus" onClick={onOpenLibrary}>
            انتخاب از رسانه‌ها
          </button>
          <button type="button" className="adm-btn adm-btn-outline adm-focus" onClick={onClose}>
            انصراف
          </button>
          <button type="submit" className="adm-btn adm-btn-primary adm-focus">
            درج تصویر
          </button>
        </div>
      </form>
    </div>
  );
}

export function isSafeImageSrc(src: string): boolean {
  const trimmed = src.trim();
  if (!trimmed || trimmed.startsWith("data:") || trimmed.includes("base64,")) return false;
  if (/^javascript:/i.test(trimmed)) return false;
  return /^(https?:\/\/|\/)(?!\/)/i.test(trimmed);
}
