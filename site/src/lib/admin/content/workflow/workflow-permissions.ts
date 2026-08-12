import type { ContentStatusValue } from "@/lib/admin/content/content-types";
import type { UserRole } from "@/types/auth";

export type WorkflowActionContext = {
  role: UserRole | null | undefined;
  authorId: string;
  currentUserId: string | null | undefined;
  status: ContentStatusValue;
};

function isOwner(ctx: WorkflowActionContext): boolean {
  return Boolean(
    ctx.currentUserId && ctx.authorId && ctx.currentUserId === ctx.authorId,
  );
}

export function canSubmitForReview(ctx: WorkflowActionContext): boolean {
  if (ctx.status !== "Draft") return false;
  if (ctx.role === "Admin") return true;
  if (ctx.role === "Writer") return isOwner(ctx);
  return false;
}

export function canApproveContent(ctx: WorkflowActionContext): boolean {
  return ctx.role === "Admin" && ctx.status === "ReviewPending";
}

export function canRejectContent(ctx: WorkflowActionContext): boolean {
  return ctx.role === "Admin" && ctx.status === "ReviewPending";
}

export function canPublishContent(ctx: WorkflowActionContext): boolean {
  return ctx.role === "Admin" && ctx.status === "Approved";
}

export function canArchiveContent(ctx: WorkflowActionContext): boolean {
  return ctx.role === "Admin" && ctx.status === "Published";
}
