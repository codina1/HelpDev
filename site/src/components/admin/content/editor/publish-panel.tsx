"use client";

import { useEffect, useRef, useState } from "react";
import { AdminSurface } from "@/components/admin/page/admin-surface";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import type { ContentStatusValue } from "@/lib/admin/content/content-types";

type PublishPanelProps = {
  status: ContentStatusValue;
  submitting: boolean;
  canMutate: boolean;
  disabledReason?: string;
  error?: unknown;
  onSaveDraft: () => void;
  onPublish: () => void;
};

/** Right column (bottom): status summary + Save Draft / Publish actions. */
export function PublishPanel({
  status,
  submitting,
  canMutate,
  disabledReason,
  error,
  onSaveDraft,
  onPublish,
}: PublishPanelProps) {
  const [confirmOpen, setConfirmOpen] = useState(false);

  const statusMessage =
    status === "Published"
      ? "این محتوا منتشر شده است"
      : "این محتوا هنوز منتشر نشده است";

  return (
    <AdminSurface className="space-y-4">
      <h2 className="adm-text text-[14px] font-bold">انتشار</h2>

      <div className="flex items-center gap-2 rounded-lg bg-[var(--adm-surface-2)] px-3 py-2.5">
        <span
          className={`h-2 w-2 rounded-full ${
            status === "Published" ? "bg-[var(--adm-success)]" : "bg-[var(--adm-warning)]"
          }`}
        />
        <span className="adm-text text-[12px] font-medium">{statusMessage}</span>
      </div>

      {!canMutate && disabledReason ? (
        <p className="rounded-lg bg-[var(--adm-warning-soft)] px-3 py-2 text-[11px] font-semibold text-[var(--adm-warning)]">
          {disabledReason}
        </p>
      ) : null}

      {error ? <AdminErrorState error={error} showHome={false} /> : null}

      <div className="flex flex-col gap-2">
        <button
          type="button"
          onClick={onSaveDraft}
          disabled={!canMutate || submitting}
          className="adm-btn adm-btn-outline adm-focus w-full justify-center"
        >
          ذخیره پیش‌نویس
        </button>
        <button
          type="button"
          onClick={() => setConfirmOpen(true)}
          disabled={!canMutate || submitting}
          className="adm-btn adm-btn-primary adm-focus w-full justify-center gap-1.5"
        >
          <AdminIcon name="check" size={16} />
          {submitting ? "در حال ذخیره..." : "انتشار"}
        </button>
      </div>

      {confirmOpen ? (
        <ConfirmDialog
          title="انتشار محتوا"
          message="آیا از انتشار این محتوا مطمئن هستید؟ محتوای منتشرشده برای کاربران قابل مشاهده خواهد بود."
          confirmLabel="انتشار"
          onConfirm={() => {
            setConfirmOpen(false);
            onPublish();
          }}
          onCancel={() => setConfirmOpen(false)}
        />
      ) : null}
    </AdminSurface>
  );
}

function ConfirmDialog({
  title,
  message,
  confirmLabel,
  onConfirm,
  onCancel,
}: {
  title: string;
  message: string;
  confirmLabel: string;
  onConfirm: () => void;
  onCancel: () => void;
}) {
  const confirmRef = useRef<HTMLButtonElement>(null);

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
        aria-labelledby="publish-confirm-title"
        className="adm-surface w-full max-w-sm space-y-4 rounded-xl p-5 shadow-[var(--adm-shadow)]"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <h3 id="publish-confirm-title" className="adm-text text-[15px] font-bold">
          {title}
        </h3>
        <p className="adm-muted text-[13px] leading-6">{message}</p>
        <div className="flex justify-end gap-2">
          <button
            type="button"
            onClick={onCancel}
            className="adm-btn adm-btn-outline adm-focus"
          >
            انصراف
          </button>
          <button
            ref={confirmRef}
            type="button"
            onClick={onConfirm}
            className="adm-btn adm-btn-primary adm-focus"
          >
            {confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
}
