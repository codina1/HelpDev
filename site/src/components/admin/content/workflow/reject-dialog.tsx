"use client";

import { useEffect, useRef, useState } from "react";

type RejectDialogProps = {
  open: boolean;
  disabled?: boolean;
  onConfirm: (comment: string) => void;
  onCancel: () => void;
};

export function RejectDialog({
  open,
  disabled = false,
  onConfirm,
  onCancel,
}: RejectDialogProps) {
  const confirmRef = useRef<HTMLButtonElement>(null);
  const [comment, setComment] = useState("");

  useEffect(() => {
    if (!open) {
      setComment("");
      return;
    }
    confirmRef.current?.focus();
    const onKey = (event: KeyboardEvent) => {
      if (event.key === "Escape") onCancel();
    };
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [open, onCancel]);

  if (!open) return null;

  const trimmed = comment.trim();
  const canConfirm = trimmed.length > 0 && !disabled;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
      onMouseDown={onCancel}
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="reject-content-title"
        className="adm-surface w-full max-w-md space-y-4 rounded-xl p-5 shadow-[var(--adm-shadow)]"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <h3 id="reject-content-title" className="adm-text text-[15px] font-bold">
          رد محتوا
        </h3>
        <p className="adm-muted text-[13px] leading-relaxed">
          محتوا به وضعیت پیش‌نویس بازمی‌گردد. توضیح رد برای نویسنده در تاریخچه گردش کار ثبت
          می‌شود.
        </p>
        <div className="space-y-1.5">
          <label htmlFor="reject-comment" className="adm-text block text-[12px] font-semibold">
            توضیح<span className="text-[var(--adm-danger)]"> *</span>
          </label>
          <textarea
            id="reject-comment"
            className="adm-input min-h-[96px] w-full text-[13px]"
            maxLength={2000}
            value={comment}
            disabled={disabled}
            onChange={(event) => setComment(event.target.value)}
            placeholder="دلیل رد را بنویسید…"
          />
        </div>
        <div className="flex flex-wrap justify-end gap-2">
          <button
            type="button"
            onClick={onCancel}
            disabled={disabled}
            className="adm-btn adm-btn-outline adm-focus"
          >
            انصراف
          </button>
          <button
            ref={confirmRef}
            type="button"
            disabled={!canConfirm}
            onClick={() => onConfirm(trimmed)}
            className="adm-btn adm-btn-primary adm-focus bg-[var(--adm-danger)] hover:opacity-90"
          >
            رد محتوا
          </button>
        </div>
      </div>
    </div>
  );
}
