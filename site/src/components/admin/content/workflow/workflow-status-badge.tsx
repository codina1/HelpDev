import { labelForWorkflowStatus, WORKFLOW_STATUS_BADGE_CLASS } from "@/lib/admin/content/workflow/workflow-labels";
import type { ContentStatusValue } from "@/lib/admin/content/content-types";

/** Workflow-focused status badge (same tokens as list badge). */
export function WorkflowStatusBadge({ status }: { status: ContentStatusValue }) {
  const tone = WORKFLOW_STATUS_BADGE_CLASS[status] ?? WORKFLOW_STATUS_BADGE_CLASS.Draft;
  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-md px-2.5 py-1 text-[12px] font-bold ${tone}`}
    >
      <span aria-hidden className="h-1.5 w-1.5 rounded-full bg-current" />
      {labelForWorkflowStatus(status)}
    </span>
  );
}
