"use client";

import {
  formatDateTimeFa,
  labelForContentType,
  shortAuthorId,
} from "@/lib/admin/content/content-mappers";
import type { ContentRevisionDetail } from "@/lib/admin/content/history/history-types";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";

type RevisionDetailPanelProps = {
  detail: ContentRevisionDetail | null;
  loading: boolean;
  error: unknown | null;
  onRetry: () => void;
  onRestore: () => void;
  onCompareWithOlder?: () => void;
  compareWithOlderDisabled?: boolean;
  restoreDisabled?: boolean;
};

export function RevisionDetailPanel({
  detail,
  loading,
  error,
  onRetry,
  onRestore,
  onCompareWithOlder,
  compareWithOlderDisabled,
  restoreDisabled,
}: RevisionDetailPanelProps) {
  if (loading) {
    return <AdminLoadingState cards={1} rows={4} />;
  }

  if (error) {
    return <AdminErrorState error={error} onRetry={onRetry} showHome={false} />;
  }

  if (!detail) {
    return (
      <div className="adm-surface rounded-xl border border-dashed border-[var(--adm-border)] p-6 text-center">
        <p className="adm-muted text-[13px]">یک نسخه از فهرست را انتخاب کنید.</p>
      </div>
    );
  }

  const { snapshot } = detail;

  return (
    <div className="adm-surface space-y-4 rounded-xl border border-[var(--adm-border)] p-4">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h3 className="adm-text text-[15px] font-bold">نسخه {detail.versionNumber}</h3>
          <p className="adm-muted mt-1 text-[12px]">
            {formatDateTimeFa(detail.createdAtUtc)} · {shortAuthorId(detail.createdByUserId)}
          </p>
          {detail.changeReason ? (
            <p className="adm-subtle mt-2 text-[12px]">دلیل: {detail.changeReason}</p>
          ) : null}
        </div>
        <div className="flex flex-wrap gap-2">
          {onCompareWithOlder ? (
            <button
              type="button"
              className="adm-btn adm-btn-outline adm-focus text-[12px]"
              disabled={compareWithOlderDisabled}
              onClick={onCompareWithOlder}
            >
              مقایسه با نسخه قدیمی‌تر
            </button>
          ) : null}
          <button
            type="button"
            className="adm-btn adm-btn-primary adm-focus text-[12px]"
            disabled={restoreDisabled}
            onClick={onRestore}
          >
            بازیابی این نسخه
          </button>
        </div>
      </div>

      <dl className="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <MetaRow label="عنوان" value={snapshot.title} />
        <MetaRow label="اسلاگ" value={snapshot.slug} />
        <MetaRow label="نوع" value={labelForContentType(snapshot.contentType)} />
        <MetaRow label="تصویر کاور" value={snapshot.coverImage ?? "—"} />
      </dl>

      <section className="space-y-1.5">
        <h4 className="adm-text text-[12px] font-semibold">خلاصه</h4>
        <p className="adm-muted whitespace-pre-wrap text-[13px] leading-relaxed">
          {snapshot.excerpt || "—"}
        </p>
      </section>

      <section className="space-y-1.5">
        <h4 className="adm-text text-[12px] font-semibold">متن</h4>
        <pre className="adm-muted max-h-64 overflow-auto whitespace-pre-wrap rounded-lg bg-[var(--adm-bg-subtle)] p-3 text-[12px] leading-relaxed">
          {snapshot.body || "—"}
        </pre>
      </section>

      <section className="space-y-2">
        <h4 className="adm-text text-[12px] font-semibold">سئو</h4>
        <dl className="grid grid-cols-1 gap-2 sm:grid-cols-2">
          <MetaRow label="عنوان سئو" value={snapshot.seoMetadata.seoTitle ?? "—"} />
          <MetaRow label="کلمه کلیدی" value={snapshot.seoMetadata.focusKeyword ?? "—"} />
          <MetaRow label="توضیح سئو" value={snapshot.seoMetadata.seoDescription ?? "—"} className="sm:col-span-2" />
          <MetaRow label="Canonical" value={snapshot.seoMetadata.canonicalUrl ?? "—"} />
          <MetaRow label="OG Image" value={snapshot.seoMetadata.ogImage ?? "—"} />
        </dl>
      </section>
    </div>
  );
}

function MetaRow({
  label,
  value,
  className = "",
}: {
  label: string;
  value: string;
  className?: string;
}) {
  return (
    <div className={className}>
      <dt className="adm-subtle text-[11px]">{label}</dt>
      <dd className="adm-text break-words text-[13px]">{value}</dd>
    </div>
  );
}
