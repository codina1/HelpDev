"use client";

import { formatDateTimeFa, shortAuthorId } from "@/lib/admin/content/content-mappers";
import type { ContentWorkflowTransition } from "@/lib/admin/content/workflow/workflow-types";
import { AdminIcon } from "@/components/admin/shared/admin-icons";

export function WorkflowTimeline({
  items,
  loading,
}: {
  items: ContentWorkflowTransition[];
  loading?: boolean;
}) {
  if (loading) {
    return (
      <div className="space-y-3" aria-busy="true">
        {Array.from({ length: 3 }).map((_, index) => (
          <div key={index} className="adm-skeleton h-16 rounded-lg" />
        ))}
      </div>
    );
  }

  if (!items.length) {
    return (
      <p className="adm-muted text-[13px] leading-relaxed">
        هنوز رویدادی در گردش کار ثبت نشده است.
      </p>
    );
  }

  return (
    <ol className="relative space-y-0 border-s border-[var(--adm-border)] pe-0 ps-4">
      {items.map((item, index) => (
        <li key={item.id} className="relative pb-6 last:pb-0">
          <span
            aria-hidden
            className="absolute -start-[9px] top-1 flex h-4 w-4 items-center justify-center rounded-full border border-[var(--adm-border)] bg-[var(--adm-surface)]"
          >
            <AdminIcon name="activity" size={10} />
          </span>
          <div className="adm-surface space-y-1 rounded-lg border border-[var(--adm-border)] p-3">
            <div className="flex flex-wrap items-center gap-2 text-[12px]">
              <span className="adm-text font-bold">{item.fromStatusLabel}</span>
              <AdminIcon name="chevron" size={14} className="rotate-180 opacity-60" />
              <span className="adm-text font-bold">{item.toStatusLabel}</span>
              <span className="adm-subtle ms-auto text-[11px]" dir="ltr">
                {formatDateTimeFa(item.createdAtUtc)}
              </span>
            </div>
            <p className="adm-muted text-[11px]">
              کاربر: <span dir="ltr">{shortAuthorId(item.actorUserId)}</span>
            </p>
            {item.comment ? (
              <p className="adm-text rounded-md bg-[var(--adm-accent-soft)] px-2 py-1.5 text-[12px] leading-relaxed">
                {item.comment}
              </p>
            ) : null}
            {index === 0 ? (
              <span className="adm-subtle text-[10px] font-semibold uppercase tracking-wide">
                آخرین تغییر
              </span>
            ) : null}
          </div>
        </li>
      ))}
    </ol>
  );
}
