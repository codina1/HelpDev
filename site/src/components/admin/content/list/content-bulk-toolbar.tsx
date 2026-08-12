"use client";

type ContentBulkToolbarProps = {
  selectedCount: number;
  onClear: () => void;
};

/**
 * Bulk action toolbar foundation.
 * No unsupported API actions are enabled — buttons explain why they are disabled.
 */
export function ContentBulkToolbar({ selectedCount, onClear }: ContentBulkToolbarProps) {
  if (selectedCount <= 0) return null;

  return (
    <div
      className="adm-surface flex flex-wrap items-center justify-between gap-3 rounded-xl border border-[var(--adm-accent)]/30 bg-[var(--adm-accent-soft)] p-3"
      role="region"
      aria-label="عملیات گروهی"
    >
      <p className="adm-text text-[13px] font-semibold">
        {selectedCount} مورد انتخاب شده
      </p>
      <div className="flex flex-wrap items-center gap-2">
        <DisabledBulkAction
          label="انتشار گروهی"
          reason="API انتشار گروهی در این نسخه وجود ندارد. هر محتوا را جداگانه منتشر کنید."
        />
        <DisabledBulkAction
          label="بایگانی گروهی"
          reason="API بایگانی گروهی پشتیبانی نمی‌شود."
        />
        <button type="button" onClick={onClear} className="adm-btn adm-btn-outline adm-focus text-[12px]">
          پاک کردن انتخاب
        </button>
      </div>
    </div>
  );
}

function DisabledBulkAction({ label, reason }: { label: string; reason: string }) {
  return (
    <button
      type="button"
      disabled
      title={reason}
      aria-disabled="true"
      className="adm-btn adm-btn-outline cursor-not-allowed text-[12px] opacity-60"
    >
      {label}
      <span className="sr-only"> — غیرفعال: {reason}</span>
    </button>
  );
}
