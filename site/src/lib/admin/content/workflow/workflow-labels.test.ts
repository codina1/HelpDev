import { describe, expect, it } from "vitest";
import { CONTENT_STATUSES } from "@/lib/admin/content/content-types";
import {
  WORKFLOW_ACTION_LABELS,
  WORKFLOW_STATUS_BADGE_CLASS,
  WORKFLOW_STATUS_LABELS,
  labelForWorkflowStatus,
} from "./workflow-labels";
import {
  canApproveContent,
  canArchiveContent,
  canPublishContent,
  canRejectContent,
  canSubmitForReview,
} from "./workflow-permissions";

describe("workflow status labels", () => {
  it("covers every backend ContentStatus with Persian labels", () => {
    for (const status of CONTENT_STATUSES) {
      expect(WORKFLOW_STATUS_LABELS[status]).toBeTruthy();
      expect(labelForWorkflowStatus(status)).toBe(WORKFLOW_STATUS_LABELS[status]);
    }
  });

  it("maps sprint copy for key workflow states", () => {
    expect(labelForWorkflowStatus("Draft")).toBe("پیش‌نویس");
    expect(labelForWorkflowStatus("ReviewPending")).toBe("در انتظار بررسی");
    expect(labelForWorkflowStatus("Approved")).toBe("تأییدشده");
    expect(labelForWorkflowStatus("Published")).toBe("منتشرشده");
    expect(labelForWorkflowStatus("Archived")).toBe("بایگانی‌شده");
  });

  it("defines badge classes for each status", () => {
    for (const status of CONTENT_STATUSES) {
      expect(WORKFLOW_STATUS_BADGE_CLASS[status]).toMatch(/var\(--adm-/);
    }
  });

  it("exposes Persian action button labels", () => {
    expect(WORKFLOW_ACTION_LABELS.submitReview).toBe("ارسال برای بررسی");
    expect(WORKFLOW_ACTION_LABELS.approve).toBe("تأیید");
    expect(WORKFLOW_ACTION_LABELS.publish).toBe("انتشار");
  });
});

describe("workflow permissions UX", () => {
  const authorId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
  const otherId = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";

  it("allows writers to submit own drafts only", () => {
    expect(
      canSubmitForReview({
        role: "Writer",
        authorId,
        currentUserId: authorId,
        status: "Draft",
      }),
    ).toBe(true);
    expect(
      canSubmitForReview({
        role: "Writer",
        authorId,
        currentUserId: otherId,
        status: "Draft",
      }),
    ).toBe(false);
  });

  it("restricts approve/reject/publish/archive to admin", () => {
    const adminCtx = { role: "Admin" as const, authorId, currentUserId: otherId, status: "ReviewPending" as const };
    expect(canApproveContent(adminCtx)).toBe(true);
    expect(canRejectContent(adminCtx)).toBe(true);
    expect(
      canApproveContent({ ...adminCtx, role: "Writer", currentUserId: authorId }),
    ).toBe(false);

    expect(
      canPublishContent({ ...adminCtx, status: "Approved" }),
    ).toBe(true);
    expect(
      canArchiveContent({ ...adminCtx, status: "Published" }),
    ).toBe(true);
  });
});
