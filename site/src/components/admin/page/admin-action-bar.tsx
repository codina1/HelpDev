import type { ReactNode } from "react";

type AdminActionBarProps = {
  filters?: ReactNode;
  actions?: ReactNode;
  selectionCount?: number;
  sticky?: boolean;
  className?: string;
};

/** Toolbar for list/table pages: filters, bulk-selection count and actions. */
export function AdminActionBar({
  filters,
  actions,
  selectionCount,
  sticky = false,
  className = "",
}: AdminActionBarProps) {
  return (
    <div
      className={`adm-surface flex flex-wrap items-center justify-between gap-3 rounded-xl p-3 ${
        sticky ? "sticky top-2 z-10" : ""
      } ${className}`.trim()}
    >
      <div className="flex flex-wrap items-center gap-2">
        {typeof selectionCount === "number" && selectionCount > 0 ? (
          <span className="adm-muted rounded-lg bg-[var(--adm-accent-soft)] px-2.5 py-1 text-[12px] font-semibold text-[var(--adm-accent-text)]">
            {selectionCount} مورد انتخاب شده
          </span>
        ) : null}
        {filters}
      </div>
      {actions ? <div className="flex items-center gap-2">{actions}</div> : null}
    </div>
  );
}
