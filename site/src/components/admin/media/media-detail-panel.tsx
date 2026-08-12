"use client";

import { useEffect, useRef } from "react";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { useAdminMediaDetail } from "@/lib/admin/media/media-hooks";
import { formatDateFa, formatFileSize, labelForMediaContentType } from "@/lib/admin/media/media-mappers";

type MediaDetailPanelProps = {
  id: string | null;
  onClose: () => void;
};

/**
 * Read-only detail drawer. The backend has no update/delete endpoint, so this
 * only ever displays information — never a storage key or filesystem path,
 * only the public URL that is already safe to share/embed.
 */
export function MediaDetailPanel({ id, onClose }: MediaDetailPanelProps) {
  const { data, loading, error, reload } = useAdminMediaDetail(id);
  const closeRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    if (!id) return;
    closeRef.current?.focus();
    const onKey = (event: KeyboardEvent) => {
      if (event.key === "Escape") onClose();
    };
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [id, onClose]);

  if (!id) return null;

  return (
    <div className="fixed inset-0 z-50 flex justify-end bg-black/50" onMouseDown={onClose}>
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="media-detail-title"
        className="adm-surface h-full w-full max-w-sm space-y-4 overflow-y-auto rounded-none p-5 shadow-[var(--adm-shadow)]"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <div className="flex items-center justify-between gap-2">
          <h3 id="media-detail-title" className="adm-text text-[15px] font-bold">
            جزئیات رسانه
          </h3>
          <button
            ref={closeRef}
            type="button"
            onClick={onClose}
            aria-label="بستن"
            className="adm-btn adm-btn-ghost adm-focus p-1.5"
          >
            <AdminIcon name="close" size={16} />
          </button>
        </div>

        {loading ? (
          <AdminLoadingState cards={0} rows={4} />
        ) : error ? (
          <AdminErrorState error={error} onRetry={reload} showHome={false} />
        ) : data ? (
          <div className="space-y-4">
            <div className="overflow-hidden rounded-xl bg-[var(--adm-surface-2)]">
              {data.absoluteUrl ? (
                // eslint-disable-next-line @next/next/no-img-element
                <img
                  src={data.absoluteUrl}
                  alt={data.altText || data.originalFileName}
                  className="max-h-64 w-full object-contain"
                />
              ) : null}
            </div>

            <dl className="space-y-2 text-[12px]">
              <DetailRow label="نام فایل" value={data.originalFileName} />
              <DetailRow label="نوع فایل" value={labelForMediaContentType(data.contentType)} />
              <DetailRow label="حجم" value={formatFileSize(data.sizeBytes)} ltr />
              <DetailRow
                label="ابعاد"
                value={data.width && data.height ? `${data.width}×${data.height}` : "—"}
                ltr
              />
              <DetailRow label="متن جایگزین" value={data.altText || "—"} />
              <DetailRow label="عنوان تصویر" value={data.caption || "—"} />
              <DetailRow label="تاریخ بارگذاری" value={formatDateFa(data.createdAtUtc)} />
            </dl>

            <div className="space-y-1.5">
              <span className="adm-text text-[12px] font-semibold">آدرس عمومی</span>
              <input
                type="text"
                readOnly
                dir="ltr"
                value={data.absoluteUrl}
                onFocus={(event) => event.currentTarget.select()}
                className="adm-input text-start text-[12px]"
              />
            </div>
          </div>
        ) : null}
      </div>
    </div>
  );
}

function DetailRow({ label, value, ltr = false }: { label: string; value: string; ltr?: boolean }) {
  return (
    <div className="flex items-center justify-between gap-3 border-b border-[var(--adm-border)] pb-2">
      <dt className="adm-subtle">{label}</dt>
      <dd dir={ltr ? "ltr" : undefined} className="adm-text max-w-[60%] truncate text-end font-semibold">
        {value}
      </dd>
    </div>
  );
}
