"use client";

import { useCallback, useMemo, useState } from "react";
import { useAuth } from "@/components/auth";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import type { ContentStatusValue } from "@/lib/admin/content/content-types";
import {
  useApproveContent,
  useArchiveContent,
  usePublishContentWorkflow,
  useRejectContent,
  useSubmitContentForReview,
} from "@/lib/admin/content/workflow/workflow-hooks";
import { WORKFLOW_ACTION_LABELS } from "@/lib/admin/content/workflow/workflow-labels";
import {
  canApproveContent,
  canArchiveContent,
  canPublishContent,
  canRejectContent,
  canSubmitForReview,
  type WorkflowActionContext,
} from "@/lib/admin/content/workflow/workflow-permissions";
import { RejectDialog } from "@/components/admin/content/workflow/reject-dialog";

export type WorkflowActionsProps = {
  contentId: string;
  authorId: string;
  status: ContentStatusValue;
  disabled?: boolean;
  layout?: "inline" | "stack";
  onStatusChange?: (status: ContentStatusValue) => void;
  onCompleted?: () => void;
};

export function WorkflowActions({
  contentId,
  authorId,
  status,
  disabled = false,
  layout = "inline",
  onStatusChange,
  onCompleted,
}: WorkflowActionsProps) {
  const { user } = useAuth();
  const submitReview = useSubmitContentForReview();
  const approve = useApproveContent();
  const reject = useRejectContent();
  const publish = usePublishContentWorkflow();
  const archive = useArchiveContent();
  const [rejectOpen, setRejectOpen] = useState(false);

  const ctx: WorkflowActionContext = useMemo(
    () => ({
      role: user?.role,
      authorId,
      currentUserId: user?.id,
      status,
    }),
    [user?.role, user?.id, authorId, status],
  );

  const busy =
    disabled ||
    submitReview.submitting ||
    approve.submitting ||
    reject.submitting ||
    publish.submitting ||
    archive.submitting;

  const afterSuccess = useCallback(
    (nextStatus: ContentStatusValue) => {
      onStatusChange?.(nextStatus);
      onCompleted?.();
    },
    [onStatusChange, onCompleted],
  );

  const run = useCallback(
    async (action: "submit" | "approve" | "publish" | "archive") => {
      if (action === "submit") {
        const detail = await submitReview.run(contentId);
        afterSuccess(detail.status);
        return;
      }
      if (action === "approve") {
        const detail = await approve.run(contentId);
        afterSuccess(detail.status);
        return;
      }
      if (action === "publish") {
        const detail = await publish.run(contentId);
        afterSuccess(detail.status);
        return;
      }
      const detail = await archive.run(contentId);
      afterSuccess(detail.status);
    },
    [submitReview, approve, publish, archive, contentId, afterSuccess],
  );

  const handleReject = useCallback(
    async (comment: string) => {
      const detail = await reject.run(contentId, { comment });
      setRejectOpen(false);
      afterSuccess(detail.status);
    },
    [reject, contentId, afterSuccess],
  );

  const showSubmit = canSubmitForReview(ctx);
  const showApprove = canApproveContent(ctx);
  const showReject = canRejectContent(ctx);
  const showPublish = canPublishContent(ctx);
  const showArchive = canArchiveContent(ctx);

  if (!showSubmit && !showApprove && !showReject && !showPublish && !showArchive) {
    return null;
  }

  const stack = layout === "stack";
  const btnClass = stack
    ? "adm-btn adm-btn-outline adm-focus w-full justify-center"
    : "adm-btn adm-btn-outline adm-focus inline-flex items-center gap-1.5";

  return (
    <>
      <div className={stack ? "flex flex-col gap-2" : "flex flex-wrap items-center gap-2"}>
        {showSubmit ? (
          <button
            type="button"
            disabled={busy}
            onClick={() => void run("submit")}
            className={`${btnClass} adm-btn-primary`}
          >
            <AdminIcon name="outbox" size={16} />
            {submitReview.submitting
              ? WORKFLOW_ACTION_LABELS.submitting
              : WORKFLOW_ACTION_LABELS.submitReview}
          </button>
        ) : null}
        {showApprove ? (
          <button
            type="button"
            disabled={busy}
            onClick={() => void run("approve")}
            className={`${btnClass} adm-btn-primary`}
          >
            <AdminIcon name="check" size={16} />
            {approve.submitting ? WORKFLOW_ACTION_LABELS.submitting : WORKFLOW_ACTION_LABELS.approve}
          </button>
        ) : null}
        {showReject ? (
          <button
            type="button"
            disabled={busy}
            onClick={() => setRejectOpen(true)}
            className={btnClass}
          >
            <AdminIcon name="close" size={16} />
            {WORKFLOW_ACTION_LABELS.reject}
          </button>
        ) : null}
        {showPublish ? (
          <button
            type="button"
            disabled={busy}
            onClick={() => void run("publish")}
            className={`${btnClass} adm-btn-primary`}
          >
            <AdminIcon name="check" size={16} />
            {publish.submitting ? WORKFLOW_ACTION_LABELS.submitting : WORKFLOW_ACTION_LABELS.publish}
          </button>
        ) : null}
        {showArchive ? (
          <button
            type="button"
            disabled={busy}
            onClick={() => void run("archive")}
            className={btnClass}
          >
            <AdminIcon name="folder" size={16} />
            {archive.submitting ? WORKFLOW_ACTION_LABELS.submitting : WORKFLOW_ACTION_LABELS.archive}
          </button>
        ) : null}
      </div>
      <RejectDialog
        open={rejectOpen}
        disabled={reject.submitting}
        onConfirm={(comment) => void handleReject(comment)}
        onCancel={() => setRejectOpen(false)}
      />
    </>
  );
}
