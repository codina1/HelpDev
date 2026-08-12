import type { ReactNode } from "react";
import Link from "next/link";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import type { ContentWorkspaceDefinition } from "@/lib/admin/content/factory";

type WorkspaceHeaderProps = {
  workspace: ContentWorkspaceDefinition;
  meta?: string;
  primaryAction?: ReactNode;
  secondaryActions?: ReactNode;
  showCreate?: boolean;
};

/** Shared header for content workspace list/create pages. */
export function WorkspaceHeader({
  workspace,
  meta,
  primaryAction,
  secondaryActions,
  showCreate = true,
}: WorkspaceHeaderProps) {
  return (
    <AdminPageHeader
      title={workspace.title}
      description={workspace.description}
      meta={meta}
      breadcrumbs={[
        { title: "محتوا", href: "/admin/content", current: false },
        { title: workspace.title, current: true },
      ]}
      primaryAction={
        primaryAction ??
        (showCreate ? (
          <Link
            href={workspace.createHref}
            className="adm-btn adm-btn-primary adm-focus inline-flex items-center gap-1.5"
          >
            <AdminIcon name="plus" size={16} />
            {workspace.createTitle}
          </Link>
        ) : undefined)
      }
      secondaryActions={secondaryActions}
    />
  );
}
