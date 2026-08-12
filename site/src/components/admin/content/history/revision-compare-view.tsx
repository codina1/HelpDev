"use client";

import { compareRevisionSnapshots, countChangedFields } from "@/lib/admin/content/history/history-compare";
import type { ContentRevisionSnapshot } from "@/lib/admin/content/history/history-types";
import { formatNumberFa } from "@/lib/admin/dashboard/dashboard-mappers";

type RevisionCompareViewProps = {
  leftLabel: string;
  rightLabel: string;
  left: ContentRevisionSnapshot;
  right: ContentRevisionSnapshot;
};

export function RevisionCompareView({ leftLabel, rightLabel, left, right }: RevisionCompareViewProps) {
  const fields = compareRevisionSnapshots(left, right);
  const changedCount = countChangedFields(fields);

  return (
    <div className="adm-surface space-y-4 rounded-xl border border-[var(--adm-border)] p-4">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h3 className="adm-text text-[14px] font-bold">مقایسه نسخه‌ها</h3>
        <p className="adm-muted text-[12px]">
          {changedCount === 0
            ? "تفاوتی یافت نشد"
            : `${formatNumberFa(changedCount)} فیلد متفاوت`}
        </p>
      </div>

      <div className="grid grid-cols-1 gap-3 lg:grid-cols-2">
        <p className="adm-subtle text-[11px] font-semibold lg:col-span-1">{leftLabel}</p>
        <p className="adm-subtle text-[11px] font-semibold lg:col-span-1">{rightLabel}</p>
      </div>

      <div className="space-y-3">
        {fields.map((field) => (
          <div
            key={field.key}
            className={`rounded-lg border p-3 ${
              field.changed
                ? "border-[var(--adm-accent)]/40 bg-[var(--adm-accent)]/5"
                : "border-[var(--adm-border)]"
            }`}
          >
            <p className="adm-text mb-2 text-[12px] font-semibold">{field.label}</p>
            <div className="grid grid-cols-1 gap-2 lg:grid-cols-2">
              <CompareCell value={field.left} multiline={field.key === "body" || field.key === "excerpt"} />
              <CompareCell value={field.right} multiline={field.key === "body" || field.key === "excerpt"} />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

function CompareCell({ value, multiline }: { value: string; multiline: boolean }) {
  const display = value || "—";
  if (multiline) {
    return (
      <pre className="adm-muted max-h-48 overflow-auto whitespace-pre-wrap break-words rounded-md bg-[var(--adm-bg-subtle)] p-2 text-[12px] leading-relaxed">
        {display}
      </pre>
    );
  }
  return (
    <p className="adm-muted break-words text-[12px] leading-relaxed">{display}</p>
  );
}
