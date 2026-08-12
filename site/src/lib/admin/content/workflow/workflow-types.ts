import type { ContentStatusValue } from "@/lib/admin/content/content-types";

/* ------------------------------- Raw API DTOs ------------------------------ */

export type ContentWorkflowTransitionRawDto = {
  id: string;
  fromStatus: string;
  toStatus: string;
  actorUserId: string;
  comment: string | null;
  createdAtUtc: string;
};

export type WorkflowHistoryRawDto = {
  items: ContentWorkflowTransitionRawDto[];
};

/* ------------------------------ View models -------------------------------- */

export type ContentWorkflowTransition = {
  id: string;
  fromStatus: ContentStatusValue;
  fromStatusLabel: string;
  toStatus: ContentStatusValue;
  toStatusLabel: string;
  actorUserId: string;
  comment: string | null;
  createdAtUtc: string;
};

export type WorkflowHistory = {
  items: ContentWorkflowTransition[];
};

export type RejectContentPayload = {
  comment: string;
};
