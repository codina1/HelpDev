"use client";

import Link from "next/link";
import { useCallback, useState, type ReactNode } from "react";
import type { ContentWorkspaceRegistryEntry } from "@/lib/admin/content/registry";
import { WorkspaceHeader } from "@/components/admin/content/workspaces/workspace-header";
import { WorkspaceStats } from "@/components/admin/content/workspaces/workspace-stats";
import { WorkspaceEmptyState } from "@/components/admin/content/workspaces/workspace-empty-state";
import { AdminPageSection } from "@/components/admin/page/admin-page-section";
import { AdminSurface } from "@/components/admin/page/admin-surface";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { getWorkspaceByKey } from "@/lib/admin/content/factory";

type FoundationWorkspaceShellProps = {
  workspace: ContentWorkspaceRegistryEntry;
  children?: ReactNode;
  /** When true, show empty list state instead of builder children. */
  mode?: "list" | "create";
};

/**
 * Shared shell for workspaces without backend persistence.
 * Save actions must stay disabled with an explicit future message.
 */
export function FoundationWorkspaceShell({
  workspace,
  children,
  mode = "list",
}: FoundationWorkspaceShellProps) {
  const def = getWorkspaceByKey(workspace.id);

  if (mode === "list") {
    return (
      <div className="space-y-6">
        <WorkspaceHeader workspace={def} />
        <WorkspaceStats workspace={def} matchingCount={0} />
        <AdminPageSection title="فهرست">
          <WorkspaceEmptyState workspace={def} />
        </AdminPageSection>
        <AdminSurface className="adm-subtle text-[12px]">
          ذخیرهٔ پایدار برای این فضای کار هنوز توسط بک‌اند پشتیبانی نمی‌شود — بدون درخواست API
          جعلی.
        </AdminSurface>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <WorkspaceHeader
        workspace={{ ...def, title: workspace.createLabel }}
        showCreate={false}
        secondaryActions={
          <Link href={workspace.route} className="adm-btn adm-btn-outline adm-focus inline-flex items-center gap-1.5">
            <AdminIcon name="chevron" size={16} />
            بازگشت
          </Link>
        }
      />
      {children}
    </div>
  );
}

export function FutureSaveBar({ label = "ذخیره" }: { label?: string }) {
  const [message, setMessage] = useState<string | null>(null);

  const onClick = useCallback(() => {
    setMessage("در نسخه آینده فعال می‌شود");
  }, []);

  return (
    <AdminSurface className="space-y-3">
      <div className="flex flex-wrap gap-2">
        <button type="button" className="adm-btn adm-btn-primary adm-focus opacity-80" onClick={onClick}>
          {label}
        </button>
        <button type="button" className="adm-btn adm-btn-outline adm-focus opacity-80" onClick={onClick}>
          ذخیره پیش‌نویس
        </button>
      </div>
      {message ? (
        <p className="text-[12px] text-[var(--adm-warning)]" role="status">
          {message}
        </p>
      ) : (
        <p className="adm-subtle text-[11px]">دکمه‌های ذخیره عمداً به API وصل نیستند.</p>
      )}
    </AdminSurface>
  );
}
