import {
  CONTENT_STATUSES,
  type ContentStatusValue,
} from "@/lib/admin/content/content-types";

/** Persian labels for backend ContentStatus values (Sprint 35 workflow). */
export const WORKFLOW_STATUS_LABELS: Record<ContentStatusValue, string> = {
  Draft: "پیش‌نویس",
  ReviewPending: "در انتظار بررسی",
  Approved: "تأییدشده",
  Published: "منتشرشده",
  Archived: "بایگانی‌شده",
};

export function labelForWorkflowStatus(status: string): string {
  return WORKFLOW_STATUS_LABELS[status as ContentStatusValue] ?? status;
}

export function isKnownContentStatus(status: string): status is ContentStatusValue {
  return (CONTENT_STATUSES as readonly string[]).includes(status);
}

/** Badge tone tokens keyed by workflow status. */
export const WORKFLOW_STATUS_BADGE_CLASS: Record<ContentStatusValue, string> = {
  Draft: "bg-[var(--adm-warning-soft)] text-[var(--adm-warning)]",
  ReviewPending: "bg-[var(--adm-info-soft)] text-[var(--adm-info)]",
  Approved: "bg-[var(--adm-accent-soft)] text-[var(--adm-accent-text)]",
  Published: "bg-[var(--adm-success-soft)] text-[var(--adm-success)]",
  Archived: "bg-[var(--adm-border)] text-[var(--adm-text-muted)]",
};

/** Persian labels for workflow action buttons. */
export const WORKFLOW_ACTION_LABELS = {
  submitReview: "ارسال برای بررسی",
  approve: "تأیید",
  reject: "رد",
  publish: "انتشار",
  archive: "بایگانی",
  submitting: "در حال انجام…",
} as const;
