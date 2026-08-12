import { labelForContentStatus } from "@/lib/admin/content/content-mappers";
import { WORKFLOW_STATUS_BADGE_CLASS } from "@/lib/admin/content/workflow/workflow-labels";
import type { ContentStatusValue } from "@/lib/admin/content/content-types";

/** Semantic badge for content publication / workflow status. */
export function ContentStatusBadge({ status }: { status: ContentStatusValue }) {
  const tone = WORKFLOW_STATUS_BADGE_CLASS[status] ?? WORKFLOW_STATUS_BADGE_CLASS.Draft;
  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-md px-2 py-0.5 text-[11px] font-bold ${tone}`}
    >
      <span aria-hidden className="h-1.5 w-1.5 rounded-full bg-current" />
      {labelForContentStatus(status)}
    </span>
  );
}
