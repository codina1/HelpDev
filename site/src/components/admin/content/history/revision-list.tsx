"use client";

import { formatDateTimeFa, shortAuthorId } from "@/lib/admin/content/content-mappers";
import { formatNumberFa } from "@/lib/admin/dashboard/dashboard-mappers";
import type { ContentRevisionListItem } from "@/lib/admin/content/history/history-types";
import { AdminEmptyState } from "@/components/admin/feedback/admin-empty-state";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";

type RevisionListProps = {
  items: ContentRevisionListItem[];
  loading: boolean;
  refreshing?: boolean;
  error: unknown | null;
  selectedVersion: number | null;
  onSelect: (version: number) => void;
  onRetry: () => void;
};

export function RevisionList({
  items,
  loading,
  refreshing,
  error,
  selectedVersion,
  onSelect,
  onRetry,
}: RevisionListProps) {
  if (loading && items.length === 0) {
    return <AdminLoadingState cards={0} rows={5} />;
  }

  if (error && items.length === 0) {
    return <AdminErrorState error={error} onRetry={onRetry} showHome={false} />;
  }

  if (!loading && items.length === 0) {
    return (
      <AdminEmptyState
        icon="content"
        title="تاریخچه‌ای ثبت نشده"
        description="پس از اولین ذخیره، نسخه‌ها اینجا نمایش داده می‌شوند."
      />
    );
  }

  return (
    <div className="adm-surface overflow-hidden rounded-xl border border-[var(--adm-border)]">
      {refreshing ? (
        <p className="adm-muted border-b border-[var(--adm-border)] px-3 py-2 text-[11px]" aria-live="polite">
          در حال به‌روزرسانی...
        </p>
      ) : null}
      <ul className="divide-y divide-[var(--adm-border)]" role="listbox" aria-label="فهرست نسخه‌ها">
        {items.map((item) => {
          const selected = item.versionNumber === selectedVersion;
          return (
            <li key={item.versionNumber}>
              <button
                type="button"
                role="option"
                aria-selected={selected}
                className={`adm-focus flex w-full flex-col gap-1 px-3 py-3 text-start transition-colors ${
                  selected
                    ? "bg-[var(--adm-accent)]/10"
                    : "hover:bg-[var(--adm-bg-subtle)]"
                }`}
                onClick={() => onSelect(item.versionNumber)}
              >
                <span className="adm-text text-[13px] font-semibold">
                  نسخه {formatNumberFa(item.versionNumber)}
                </span>
                <span className="adm-muted text-[11px]">
                  {formatDateTimeFa(item.createdAtUtc)} · {shortAuthorId(item.createdByUserId)}
                </span>
                {item.changeReason ? (
                  <span className="adm-subtle line-clamp-2 text-[11px]">{item.changeReason}</span>
                ) : null}
              </button>
            </li>
          );
        })}
      </ul>
    </div>
  );
}
