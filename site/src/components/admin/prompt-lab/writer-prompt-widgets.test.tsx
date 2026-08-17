import { renderToStaticMarkup } from "react-dom/server";
import { describe, expect, it } from "vitest";
import { existsSync, readFileSync } from "node:fs";
import { join } from "node:path";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { WriterPromptStatusBadge } from "@/components/admin/prompt-lab/writer-prompt-status-badge";
import { WriterPromptTable } from "@/components/admin/prompt-lab/writer-prompt-table";
import { WriterPromptEmptyState } from "@/components/admin/prompt-lab/writer-prompt-empty-state";
import type { WriterPromptListItem } from "@/lib/admin/prompt-lab/writer-prompt-types";

const sampleItem: WriterPromptListItem = {
  id: "11111111-2222-3333-4444-555555555555",
  title: "پرامپت نمونه",
  slug: "sample-prompt",
  status: "Draft",
  statusLabel: "پیش‌نویس",
  views: 12,
  copyCount: 4,
  createdAt: "2026-07-01T00:00:00Z",
};

describe("Writer Prompt Studio widgets", () => {
  it("is mounted at /admin/prompt-lab", () => {
    expect(existsSync(join(process.cwd(), "src/app/admin/prompt-lab/page.tsx"))).toBe(true);
    const page = readFileSync(join(process.cwd(), "src/app/admin/prompt-lab/page.tsx"), "utf8");
    expect(page).toContain("WriterPromptDashboard");
  });

  it("renders status badges", () => {
    expect(renderToStaticMarkup(<WriterPromptStatusBadge status="Approved" />)).toContain(
      "منتشرشده",
    );
    expect(renderToStaticMarkup(<WriterPromptStatusBadge status="Rejected" />)).toContain("ردشده");
  });

  it("renders table columns", () => {
    const html = renderToStaticMarkup(<WriterPromptTable items={[sampleItem]} />);
    expect(html).toContain("پرامپت نمونه");
    expect(html).toContain("sample-prompt");
    expect(html).toContain("عنوان");
    expect(html).toContain("وضعیت");
    expect(html).toContain("تاریخ ایجاد");
    expect(html).toContain("کپی");
    expect(html).toContain("بازدید");
    expect(html).toContain("پیش‌نویس");
  });

  it("renders empty state CTA", () => {
    const html = renderToStaticMarkup(<WriterPromptEmptyState filtered={false} />);
    expect(html).toContain("ایجاد پرامپت");
    expect(html).toContain(ADMIN_ROUTES.contentPromptsNew);
  });
});
