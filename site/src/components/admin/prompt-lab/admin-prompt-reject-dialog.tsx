"use client";

import { useEffect, useRef, useState } from "react";

type AdminPromptRejectDialogProps = {
  open: boolean;
  title?: string;
  disabled?: boolean;
  onConfirm: (reason: string) => void;
  onCancel: () => void;
};

/** Required-reason dialog for rejecting a writer prompt. */
export function AdminPromptRejectDialog({
  open,
  title = "رد پرامپت",
  disabled = false,
  onConfirm,
  onCancel,
}: AdminPromptRejectDialogProps) {
  const confirmRef = useRef<HTMLButtonElement>(null);
  const [reason, setReason] = useState("");

  useEffect(() => {
    if (!open) {
      setReason("");
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

  const trimmed = reason.trim();
  const canConfirm = trimmed.length > 0 && !disabled;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
      onMouseDown={onCancel}
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="reject-prompt-title"
        className="adm-surface w-full max-w-md space-y-4 rounded-xl p-5 shadow-[var(--adm-shadow)]"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <h3 id="reject-prompt-title" className="adm-text text-[15px] font-bold">
          {title}
        </h3>
        <p className="adm-muted text-[13px] leading-relaxed">
          پرامپت رد می‌شود و برای نویسنده قابل انتشار نیست. دلیل رد الزامی است.
        </p>
        <div className="space-y-1.5">
          <label htmlFor="reject-prompt-reason" className="adm-text block text-[12px] font-semibold">
            دلیل رد<span className="text-[var(--adm-danger)]"> *</span>
          </label>
          <textarea
            id="reject-prompt-reason"
            className="adm-input min-h-[96px] w-full text-[13px]"
            maxLength={2000}
            value={reason}
            disabled={disabled}
            onChange={(event) => setReason(event.target.value)}
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
            رد پرامپت
          </button>
        </div>
      </div>
    </div>
  );
}
