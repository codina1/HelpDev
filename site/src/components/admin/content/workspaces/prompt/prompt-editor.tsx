"use client";

import Link from "next/link";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { getWorkspaceByKey } from "@/lib/admin/content/factory";
import { WorkspaceHeader } from "@/components/admin/content/workspaces/workspace-header";
import { AdminSurface } from "@/components/admin/page/admin-surface";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { WorkspaceNote } from "@/components/admin/content/workspaces/future-capability-list";

const workspace = getWorkspaceByKey("prompt");

/** Create prompt — form UI is planned; list lives in Writer Prompt Studio. */
export function PromptEditor() {
  return (
    <div className="space-y-6">
      <WorkspaceHeader workspace={{ ...workspace, title: workspace.createTitle }} showCreate={false} />
      <AdminSurface className="space-y-4">
        <div className="flex items-start gap-3">
          <AdminIcon name="prompt" size={24} />
          <div className="space-y-2">
            <h2 className="adm-text text-[15px] font-bold">ایجاد پرامپت</h2>
            <p className="adm-muted text-[13px] leading-6">
              فرم ایجاد/ویرایش پرامپت به‌زودی در همین فضای کار اضافه می‌شود. تا آن زمان
              پرامپت‌های خود را از داشبورد Writer Prompt Studio مدیریت کنید.
            </p>
          </div>
        </div>
        <WorkspaceNote>
          ایجاد Content با نوع Prompt از این مسیر انجام نمی‌شود تا با قرارداد Prompt Lab
          تداخل نداشته باشد.
        </WorkspaceNote>
        <div className="flex flex-wrap gap-2">
          <Link href={ADMIN_ROUTES.promptLab} className="adm-btn adm-btn-primary adm-focus">
            بازگشت به داشبورد
          </Link>
          <Link href={workspace.listHref} className="adm-btn adm-btn-outline adm-focus">
            فهرست پرامپت‌ها
          </Link>
        </div>
      </AdminSurface>
    </div>
  );
}
