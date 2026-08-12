import type { ReactNode } from "react";
import type { AdminIconName } from "@/lib/admin/navigation";
import { AdminIcon } from "@/components/admin/shared/admin-icons";

type AdminEmptyStateProps = {
  title: string;
  description?: string;
  icon?: AdminIconName;
  primaryAction?: ReactNode;
  secondaryAction?: ReactNode;
};

/** Consistent empty-state placeholder to avoid vague blank areas. */
export function AdminEmptyState({
  title,
  description,
  icon = "content",
  primaryAction,
  secondaryAction,
}: AdminEmptyStateProps) {
  return (
    <div className="adm-surface flex flex-col items-center gap-3 rounded-xl p-10 text-center">
      <span className="flex h-12 w-12 items-center justify-center rounded-full bg-[var(--adm-surface-3)] text-[var(--adm-text-muted)]">
        <AdminIcon name={icon} size={24} />
      </span>
      <div className="space-y-1">
        <h3 className="adm-text text-[14px] font-bold">{title}</h3>
        {description ? (
          <p className="adm-muted max-w-sm text-[12px] leading-6">{description}</p>
        ) : null}
      </div>
      {primaryAction || secondaryAction ? (
        <div className="flex flex-wrap items-center justify-center gap-2 pt-1">
          {primaryAction}
          {secondaryAction}
        </div>
      ) : null}
    </div>
  );
}
