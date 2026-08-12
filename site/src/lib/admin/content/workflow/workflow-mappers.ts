import { normalizeContentStatus } from "@/lib/admin/content/content-mappers";
import { labelForWorkflowStatus } from "@/lib/admin/content/workflow/workflow-labels";
import type {
  ContentWorkflowTransition,
  ContentWorkflowTransitionRawDto,
  WorkflowHistory,
  WorkflowHistoryRawDto,
} from "@/lib/admin/content/workflow/workflow-types";

function mapTransition(raw: ContentWorkflowTransitionRawDto): ContentWorkflowTransition {
  const fromStatus = normalizeContentStatus(raw.fromStatus);
  const toStatus = normalizeContentStatus(raw.toStatus);
  return {
    id: raw.id,
    fromStatus,
    fromStatusLabel: labelForWorkflowStatus(fromStatus),
    toStatus,
    toStatusLabel: labelForWorkflowStatus(toStatus),
    actorUserId: raw.actorUserId,
    comment: raw.comment,
    createdAtUtc: raw.createdAtUtc,
  };
}

export function mapWorkflowHistory(raw: WorkflowHistoryRawDto): WorkflowHistory {
  return {
    items: raw.items.map(mapTransition),
  };
}
