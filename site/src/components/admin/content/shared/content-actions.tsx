"use client";

import { useEffect, useRef, useState } from "react";
import Link from "next/link";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import type { ContentStatusValue } from "@/lib/admin/content/content-types";

type ContentActionsProps = {
  id: string;
  title: string;
  status: ContentStatusValue;
  /** Hide the edit action (e.g. when already on the edit page). */
  showEdit?: boolean;
  compact?: boolean;
  publishing?: boolean;
  publishError?: unknown;
  onPublish?: () => Promise<void> | void;
};

/**
 * Row/detail actions for a content item. Admin navigation always uses the
 * content id. Publish is available for Draft rows only. There is no safe
 * public slug route on the site yet, so no public-preview link is offered.
 */
export function ContentActions({
  id,
  title,
  status,
  showEdit = true,
  compact = false,
  publishing = false,
  publishError,
  onPublish,
}: ContentActionsProps) {
  const [confirmOpen, setConfirmOpen] = useState(false);
  const size = compact ? "px-2.5 py-1 text-[11px]" : "px-3 py-1.5 text-[12px]";
  const detailHref = `${ADMIN_ROUTES.content}/${encodeURIComponent(id)}`;
  const canPublish = status === "Draft" && typeof onPublish === "function";

  return (
    <div className="space-y-2">
      <div className="flex flex-wrap items-center gap-2">
        <Link href={detailHref} className={`adm-btn adm-btn-ghost adm-focus ${size}`}>
          <AdminIcon name="content" size={14} />
          مشاهده
        </Link>
        {showEdit ? (
          <Link href={`${detailHref}/edit`} className={`adm-btn adm-btn-outline adm-focus ${size}`}>
            <AdminIcon name="content" size={14} />
            ویرایش
          </Link>
        ) : null}
        {canPublish ? (
          <button
            type="button"
            className={`adm-btn adm-btn-primary adm-focus ${size}`}
            disabled={publishing}
            onClick={() => setConfirmOpen(true)}
          >
            <AdminIcon name="check" size={14} />
            {publishing ? "در حال انتشار..." : "انتشار"}
          </button>
        ) : null}
      </div>

      {publishError ? <AdminErrorState error={publishError} showHome={false} /> : null}

      {confirmOpen ? (
        <PublishConfirmDialog
          title={title}
          disabled={publishing}
          onConfirm={async () => {
            setConfirmOpen(false);
            await onPublish?.();
          }}
          onCancel={() => setConfirmOpen(false)}
        />
      ) : null}
    </div>
  );
}

function PublishConfirmDialog({
  title,
  disabled,
  onConfirm,
  onCancel,
}: {
  title: string;
  disabled: boolean;
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
        aria-labelledby="list-publish-confirm-title"
        className="adm-surface w-full max-w-sm space-y-4 rounded-xl p-5 shadow-[var(--adm-shadow)]"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <h3 id="list-publish-confirm-title" className="adm-text text-[15px] font-bold">
          انتشار محتوا
        </h3>
        <p className="adm-muted text-[13px] leading-6">
          آیا از انتشار «{title}» مطمئن هستید؟ محتوای منتشرشده برای کاربران قابل مشاهده خواهد بود.
        </p>
        <div className="flex justify-end gap-2">
          <button type="button" onClick={onCancel} className="adm-btn adm-btn-outline adm-focus" disabled={disabled}>
            انصراف
          </button>
          <button
            ref={confirmRef}
            type="button"
            onClick={onConfirm}
            disabled={disabled}
            className="adm-btn adm-btn-primary adm-focus"
          >
            انتشار
          </button>
        </div>
      </div>
    </div>
  );
}
