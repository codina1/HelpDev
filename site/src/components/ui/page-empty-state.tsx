import type { ReactNode } from "react";

type PageEmptyStateProps = {
  title: string;
  description?: string;
  icon?: ReactNode;
  action?: ReactNode;
  className?: string;
};

/** Shared empty state for public + product pages (RTL-friendly). Token-driven. */
export function PageEmptyState({
  title,
  description,
  icon,
  action,
  className = "",
}: PageEmptyStateProps) {
  return (
    <div
      dir="rtl"
      className={`ds-surface flex flex-col items-center gap-3 px-6 py-10 text-center ${className}`.trim()}
      role="status"
    >
      {icon ? (
        <span
          className="flex h-12 w-12 items-center justify-center rounded-full bg-[color:color-mix(in_srgb,var(--ds-primary)_14%,transparent)] text-[color:var(--ds-muted)]"
          aria-hidden
        >
          {icon}
        </span>
      ) : null}
      <div className="space-y-1">
        <h3 className="text-[15px] font-bold text-[color:var(--ds-fg)]">{title}</h3>
        {description ? (
          <p className="mx-auto max-w-md text-[13px] leading-6 text-[color:var(--ds-muted)]">{description}</p>
        ) : null}
      </div>
      {action ? <div className="pt-1">{action}</div> : null}
    </div>
  );
}
