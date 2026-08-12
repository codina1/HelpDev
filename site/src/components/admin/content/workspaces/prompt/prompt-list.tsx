"use client";

import Link from "next/link";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { getWorkspaceByKey } from "@/lib/admin/content/factory";
import { WorkspaceHeader } from "@/components/admin/content/workspaces/workspace-header";
import { AdminSurface } from "@/components/admin/page/admin-surface";
import { WorkspaceNote } from "@/components/admin/content/workspaces/future-capability-list";

const workspace = getWorkspaceByKey("prompt");

/** Prompt workspace list — delegates to Prompt Lab (no Content Prompt duplication). */
export function PromptList() {
  return (
    <div className="space-y-6">
      <WorkspaceHeader
        workspace={workspace}
        primaryAction={
          <Link href={ADMIN_ROUTES.promptLab} className="adm-btn adm-btn-primary adm-focus">
            باز کردن Prompt Lab
          </Link>
        }
      />
      <AdminSurface className="space-y-3">
        <h2 className="adm-text text-[14px] font-bold">پیوند به Prompt Lab</h2>
        <p className="adm-muted text-[13px] leading-6">
          فهرست و ویرایش پرامپت‌ها در ماژول Prompt Lab نگهداری می‌شود. این صفحه فقط مسیر فضای
          کار محتوا را به آن ماژول وصل می‌کند.
        </p>
        <WorkspaceNote>
          ایجاد Content با نوع Prompt از این مسیر انجام نمی‌شود تا قرارداد Prompt Lab حفظ شود.
        </WorkspaceNote>
      </AdminSurface>
    </div>
  );
}
