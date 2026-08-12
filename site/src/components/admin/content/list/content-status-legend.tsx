import { CONTENT_STATUSES } from "@/lib/admin/content/content-types";
import { WORKFLOW_STATUS_LABELS } from "@/lib/admin/content/workflow/workflow-labels";
import { ContentStatusBadge } from "@/components/admin/content/list/content-status-badge";

/** Status legend: color + text labels (not color-only). */
export function ContentStatusLegend() {
  return (
    <div
      className="flex flex-wrap items-center gap-2"
      role="list"
      aria-label="راهنمای وضعیت محتوا"
    >
      {CONTENT_STATUSES.map((status) => (
        <div key={status} className="flex items-center gap-1.5" role="listitem">
          <ContentStatusBadge status={status} />
          <span className="sr-only">{WORKFLOW_STATUS_LABELS[status]}</span>
        </div>
      ))}
    </div>
  );
}
