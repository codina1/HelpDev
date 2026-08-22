"use client";

import { useEffect, useRef, useState } from "react";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import {
  useAdminMediaDetail,
  useDeleteMediaAsset,
  useUpdateMediaAsset,
} from "@/lib/admin/media/media-hooks";
import { formatDateFa, formatFileSize, labelForMediaContentType } from "@/lib/admin/media/media-mappers";
import { validateAltText, validateCaption } from "@/lib/admin/media/media-validation";

type MediaDetailPanelProps = {
  id: string | null;
  onClose: () => void;
  onChanged?: () => void;
};

/**
 * Media detail drawer. Supports editing alt/caption and confirmed delete.
 * Never exposes a storage key or filesystem path — only the public URL.
 */
export function MediaDetailPanel({ id, onClose, onChanged }: MediaDetailPanelProps) {
  const { data, loading, error, reload } = useAdminMediaDetail(id);
  const update = useUpdateMediaAsset();
  const remove = useDeleteMediaAsset();
  const closeRef = useRef<HTMLButtonElement>(null);
  const [altText, setAltText] = useState("");
  const [caption, setCaption] = useState("");
  const [confirmDelete, setConfirmDelete] = useState(false);
  const [localError, setLocalError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    closeRef.current?.focus();
    const onKey = (event: KeyboardEvent) => {
      if (event.key === "Escape") onClose();
    };
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [id, onClose]);

  useEffect(() => {
    if (!data) return;
    setAltText(data.altText);
    setCaption(data.caption);
    setConfirmDelete(false);
    setLocalError(null);
  }, [data]);

  if (!id) return null;

  const handleSaveMetadata = async () => {
    const altError = validateAltText(altText);
    const captionError = validateCaption(caption);
    if (altError || captionError) {
      setLocalError(altError ?? captionError);
      return;
    }
    setLocalError(null);
    await update.update(id, {
      altText: altText.trim() || null,
      caption: caption.trim() || null,
    });
    reload();
    onChanged?.();
  };

  const handleDelete = async () => {
    await remove.remove(id);
    onChanged?.();
    onClose();
  };

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

            <label className="block space-y-1.5">
              <span className="adm-text text-[12px] font-semibold">متن جایگزین</span>
              <input
                className="adm-input"
                value={altText}
                onChange={(event) => setAltText(event.target.value)}
              />
            </label>
            <label className="block space-y-1.5">
              <span className="adm-text text-[12px] font-semibold">عنوان تصویر</span>
              <input
                className="adm-input"
                value={caption}
                onChange={(event) => setCaption(event.target.value)}
              />
            </label>
            {localError ? (
              <p className="text-[11px] font-semibold text-[var(--adm-danger)]">{localError}</p>
            ) : null}
            <button
              type="button"
              className="adm-btn adm-btn-primary adm-focus w-full"
              disabled={update.submitting}
              onClick={() => void handleSaveMetadata()}
            >
              ذخیره متن جایگزین و عنوان
            </button>

            <div className="rounded-xl border border-[var(--adm-danger-soft)] p-3">
              {confirmDelete ? (
                <div className="space-y-2">
                  <p className="text-[12px] font-semibold text-[var(--adm-danger)]">
                    این رسانه برای همیشه حذف می‌شود. ادامه می‌دهید؟
                  </p>
                  <div className="flex gap-2">
                    <button
                      type="button"
                      className="adm-btn adm-btn-ghost adm-focus flex-1"
                      onClick={() => setConfirmDelete(false)}
                    >
                      انصراف
                    </button>
                    <button
                      type="button"
                      className="adm-btn adm-focus flex-1 bg-[var(--adm-danger-soft)] text-[var(--adm-danger)]"
                      disabled={remove.submitting}
                      onClick={() => void handleDelete()}
                    >
                      تأیید حذف
                    </button>
                  </div>
                </div>
              ) : (
                <button
                  type="button"
                  className="adm-btn adm-btn-ghost adm-focus w-full text-[var(--adm-danger)]"
                  onClick={() => setConfirmDelete(true)}
                >
                  حذف رسانه
                </button>
              )}
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
