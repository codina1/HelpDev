import type { ReactNode } from "react";
import {
  AdminBreadcrumb,
  type AdminBreadcrumbProps,
} from "@/components/admin/navigation/admin-breadcrumb";

type AdminPageHeaderProps = {
  title: string;
  description?: string;
  breadcrumbs?: AdminBreadcrumbProps["items"];
  primaryAction?: ReactNode;
  secondaryActions?: ReactNode;
  badge?: ReactNode;
  meta?: ReactNode;
};

/** Standard header for every Admin page: title, context, actions and meta. */
export function AdminPageHeader({
  title,
  description,
  breadcrumbs,
  primaryAction,
  secondaryActions,
  badge,
  meta,
}: AdminPageHeaderProps) {
  return (
    <header className="space-y-3">
      {breadcrumbs && breadcrumbs.length > 0 ? (
        <AdminBreadcrumb items={breadcrumbs} />
      ) : null}

      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div className="min-w-0 space-y-1">
          <div className="flex flex-wrap items-center gap-2">
            <h1 className="adm-text text-xl font-bold">{title}</h1>
            {badge}
          </div>
          {description ? (
            <p className="adm-muted max-w-2xl text-[13px] leading-6">
              {description}
            </p>
          ) : null}
          {meta ? <div className="adm-subtle text-[12px]">{meta}</div> : null}
        </div>

        {primaryAction || secondaryActions ? (
          <div className="flex shrink-0 flex-wrap items-center gap-2">
            {secondaryActions}
            {primaryAction}
          </div>
        ) : null}
      </div>
    </header>
  );
}
