import { renderToStaticMarkup } from "react-dom/server";
import { describe, expect, it } from "vitest";
import { existsSync, readFileSync } from "node:fs";
import { join } from "node:path";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { AdminPromptReviewTable } from "@/components/admin/prompt-lab/admin-prompt-review-table";
import { AdminPromptReviewTabs } from "@/components/admin/prompt-lab/admin-prompt-review-tabs";
import { AdminPromptRejectDialog } from "@/components/admin/prompt-lab/admin-prompt-reject-dialog";
import type { AdminPromptReviewItem } from "@/lib/admin/prompt-lab/admin-prompt-review-types";
import { DEFAULT_ADMIN_PROMPT_REVIEW_QUERY } from "@/lib/admin/prompt-lab/admin-prompt-review-types";

const pendingItem: AdminPromptReviewItem = {
  id: "p1",
  title: "بازبینی مرز سیستم",
  slug: "system-boundary-review",
  authorId: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  categoryName: "Coding",
  preview: "You are a reviewer…",
  status: "Submitted",
  rejectionReason: null,
};

describe("Admin Prompt Review", () => {
  it("is mounted at /admin/prompts", () => {
    expect(ADMIN_ROUTES.prompts).toBe("/admin/prompts");
    expect(existsSync(join(process.cwd(), "src/app/admin/prompts/page.tsx"))).toBe(true);
    const page = readFileSync(join(process.cwd(), "src/app/admin/prompts/page.tsx"), "utf8");
    expect(page).toContain("AdminPromptReviewDashboard");
  });

  it("renders pending, published, and rejected tabs", () => {
    const html = renderToStaticMarkup(<AdminPromptReviewTabs query={DEFAULT_ADMIN_PROMPT_REVIEW_QUERY} />);
    expect(html).toContain("در انتظار");
    expect(html).toContain("منتشرشده");
    expect(html).toContain("ردشده");
    expect(html).toContain("/admin/prompts?tab=published");
    expect(html).toContain("/admin/prompts?tab=rejected");
  });

  it("renders pending columns and actions", () => {
    const html = renderToStaticMarkup(
      <AdminPromptReviewTable items={[pendingItem]} showActions onApprove={() => {}} onReject={() => {}} />,
    );
    expect(html).toContain("بازبینی مرز سیستم");
    expect(html).toContain("عنوان");
    expect(html).toContain("نویسنده");
    expect(html).toContain("دسته‌بندی");
    expect(html).toContain("پیش‌نمایش");
    expect(html).toContain("تأیید");
    expect(html).toContain("رد");
    expect(html).toContain("کدنویسی");
  });

  it("requires a reason in the reject dialog", () => {
    const html = renderToStaticMarkup(
      <AdminPromptRejectDialog open title="رد پرامپت" onConfirm={() => {}} onCancel={() => {}} />,
    );
    expect(html).toContain("دلیل رد");
    expect(html).toContain("رد پرامپت");
  });
});
