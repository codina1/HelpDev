"use client";

import { useEffect, useRef, useState } from "react";

type RestoreConfirmDialogProps = {
  versionNumber: number;
  disabled?: boolean;
  onConfirm: (changeReason: string | null) => void;
  onCancel: () => void;
};

export function RestoreConfirmDialog({
  versionNumber,
  disabled = false,
  onConfirm,
  onCancel,
}: RestoreConfirmDialogProps) {
  const confirmRef = useRef<HTMLButtonElement>(null);
  const [reason, setReason] = useState("");

  useEffect(() => {
    confirmRef.current?.focus();
    const onKey = (event: KeyboardEvent) => {
      if (event.key === "Escape") onCancel();
    };
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [onCancel]);

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
      onMouseDown={onCancel}
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="restore-revision-title"
        className="adm-surface w-full max-w-md space-y-4 rounded-xl p-5 shadow-[var(--adm-shadow)]"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <h3 id="restore-revision-title" className="adm-text text-[15px] font-bold">
          بازیابی نسخه {versionNumber}
        </h3>
        <p className="adm-muted text-[13px] leading-relaxed">
          محتوای فعلی با اسنپ‌شات این نسخه جایگزین می‌شود. یک نسخه جدید از وضعیت قبل از بازیابی در
          تاریخچه ثبت می‌شود.
        </p>
        <div className="space-y-1.5">
          <label htmlFor="restore-change-reason" className="adm-text block text-[12px] font-semibold">
            دلیل (اختیاری)
          </label>
          <textarea
            id="restore-change-reason"
            className="adm-input min-h-[72px] w-full text-[13px]"
            maxLength={500}
            value={reason}
            disabled={disabled}
            onChange={(event) => setReason(event.target.value)}
            placeholder="مثلاً بازگردانی پس از ویرایش اشتباه"
          />
        </div>
        <div className="flex flex-wrap justify-end gap-2">
          <button
            type="button"
            className="adm-btn adm-btn-outline adm-focus"
            disabled={disabled}
            onClick={onCancel}
          >
            انصراف
          </button>
          <button
            ref={confirmRef}
            type="button"
            className="adm-btn adm-btn-primary adm-focus"
            disabled={disabled}
            onClick={() => onConfirm(reason.trim() || null)}
          >
            {disabled ? "در حال بازیابی..." : "بازیابی"}
          </button>
        </div>
      </div>
    </div>
  );
}
