"use client";

import Link from "next/link";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { getWorkspaceByKey } from "@/lib/admin/content/factory";
import { WorkspaceHeader } from "@/components/admin/content/workspaces/workspace-header";
import { AdminSurface } from "@/components/admin/page/admin-surface";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { WorkspaceNote } from "@/components/admin/content/workspaces/future-capability-list";

const workspace = getWorkspaceByKey("prompt");

/**
 * Prompt workspace does not duplicate PromptLab.
 * It links to the existing Prompt Lab Admin module.
 */
export function PromptEditor() {
  return (
    <div className="space-y-6">
      <WorkspaceHeader workspace={{ ...workspace, title: workspace.createTitle }} showCreate={false} />
      <AdminSurface className="space-y-4">
        <div className="flex items-start gap-3">
          <AdminIcon name="prompt" size={24} />
          <div className="space-y-2">
            <h2 className="adm-text text-[15px] font-bold">مدیریت پرامپت در Prompt Lab</h2>
            <p className="adm-muted text-[13px] leading-6">
              موجودیت پرامپت متعلق به ماژول Prompt Lab است. برای جلوگیری از تکرار و دادهٔ
              ساختگی، ایجاد/ویرایش از همان فضای کار انجام می‌شود.
            </p>
          </div>
        </div>
        <WorkspaceNote>
          از ایجاد Content با نوع Prompt در این مسیر خودداری شده تا با قرارداد Prompt Lab
          تداخل نداشته باشد.
        </WorkspaceNote>
        <div className="flex flex-wrap gap-2">
          <Link href={ADMIN_ROUTES.promptLab} className="adm-btn adm-btn-primary adm-focus">
            رفتن به Prompt Lab
          </Link>
          <Link href={workspace.listHref} className="adm-btn adm-btn-outline adm-focus">
            بازگشت
          </Link>
        </div>
      </AdminSurface>
    </div>
  );
}
