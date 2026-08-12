"use client";

import Link from "next/link";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import type { ContentStatusValue } from "@/lib/admin/content/content-types";
import { WorkflowStatusBadge } from "@/components/admin/content/workflow/workflow-status-badge";
import { WorkflowActions } from "@/components/admin/content/workflow/workflow-actions";
import { WorkflowTimeline } from "@/components/admin/content/workflow/workflow-timeline";
import { useContentWorkflowHistory } from "@/lib/admin/content/workflow/workflow-hooks";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { AdminIcon } from "@/components/admin/shared/admin-icons";

export type WorkflowPanelProps = {
  contentId: string;
  authorId: string;
  status: ContentStatusValue;
  onStatusChange?: (status: ContentStatusValue) => void;
  showTimeline?: boolean;
  compact?: boolean;
};

/** Studio banner or full workflow tab body. */
export function WorkflowPanel({
  contentId,
  authorId,
  status,
  onStatusChange,
  showTimeline = false,
  compact = false,
}: WorkflowPanelProps) {
  const history = useContentWorkflowHistory(showTimeline ? contentId : null);

  const handleCompleted = () => {
    if (showTimeline) history.reload();
  };

  return (
    <div
      className={`rounded-xl border border-[var(--adm-border)] bg-[var(--adm-surface)] ${
        compact ? "px-3 py-2.5" : "space-y-4 p-4"
      }`}
    >
      <div
        className={`flex flex-wrap items-center gap-3 ${
          compact ? "justify-between" : "justify-between gap-4"
        }`}
      >
        <div className="flex flex-wrap items-center gap-2">
          <span className="adm-muted text-[12px] font-semibold">وضعیت گردش کار</span>
          <WorkflowStatusBadge status={status} />
        </div>
        <WorkflowActions
          contentId={contentId}
          authorId={authorId}
          status={status}
          onStatusChange={onStatusChange}
          onCompleted={handleCompleted}
          layout="inline"
        />
      </div>
      {!compact ? (
        <p className="adm-muted text-[12px] leading-relaxed">
          پیش‌نویس ← ارسال برای بررسی ← تأیید ادمین ← انتشار ← بایگانی. جزئیات کامل در{" "}
          <Link
            href={`${ADMIN_ROUTES.content}/${encodeURIComponent(contentId)}/workflow`}
            className="adm-focus font-semibold text-[var(--adm-accent-text)] underline-offset-2 hover:underline"
          >
            تب گردش کار
          </Link>
          .
        </p>
      ) : null}
      {showTimeline ? (
        <div className="space-y-3 border-t border-[var(--adm-border)] pt-4">
          <h3 className="adm-text flex items-center gap-1.5 text-[13px] font-bold">
            <AdminIcon name="activity" size={16} />
            تاریخچه گردش کار
          </h3>
          {history.error ? (
            <AdminErrorState error={history.error} onRetry={history.reload} />
          ) : (
            <WorkflowTimeline items={history.data?.items ?? []} loading={history.loading} />
          )}
        </div>
      ) : null}
    </div>
  );
}
