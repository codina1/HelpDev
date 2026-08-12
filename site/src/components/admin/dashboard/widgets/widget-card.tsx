import type { ReactNode } from "react";
import type { AdminIconName } from "@/lib/admin/navigation";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { AdminEmptyState } from "@/components/admin/feedback/admin-empty-state";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";

type WidgetCardProps = {
  title: string;
  icon?: AdminIconName;
  headerAction?: ReactNode;
  loading?: boolean;
  error?: unknown;
  isEmpty?: boolean;
  emptyTitle?: string;
  emptyDescription?: string;
  emptyIcon?: AdminIconName;
  onRetry?: () => void;
  /** Optional custom skeleton; defaults to a few shimmer lines. */
  skeleton?: ReactNode;
  children: ReactNode;
  className?: string;
};

/**
 * Shared dashboard widget shell. Centralizes the Loading / Error / Empty /
 * Success state machine so every widget behaves consistently and reuses the
 * Admin feedback components.
 */
export function WidgetCard({
  title,
  icon,
  headerAction,
  loading = false,
  error = null,
  isEmpty = false,
  emptyTitle = "داده‌ای برای نمایش نیست",
  emptyDescription,
  emptyIcon = "content",
  onRetry,
  skeleton,
  children,
  className = "",
}: WidgetCardProps) {
  return (
    <section className={`adm-surface flex flex-col rounded-xl ${className}`.trim()}>
      <header className="flex items-center justify-between gap-3 border-b border-[var(--adm-border)] px-4 py-3">
        <div className="flex items-center gap-2">
          {icon ? (
            <span className="flex h-7 w-7 items-center justify-center rounded-lg bg-[var(--adm-surface-3)] text-[var(--adm-text-muted)]">
              <AdminIcon name={icon} size={16} />
            </span>
          ) : null}
          <h2 className="adm-text text-[14px] font-bold">{title}</h2>
        </div>
        {headerAction ? <div className="shrink-0">{headerAction}</div> : null}
      </header>

      <div className="flex-1 p-4">
        {loading ? (
          skeleton ?? <DefaultSkeleton />
        ) : error ? (
          <AdminErrorState error={error} onRetry={onRetry} showHome={false} />
        ) : isEmpty ? (
          <AdminEmptyState
            title={emptyTitle}
            description={emptyDescription}
            icon={emptyIcon}
          />
        ) : (
          children
        )}
      </div>
    </section>
  );
}

function DefaultSkeleton() {
  return (
    <div className="space-y-3" role="status" aria-live="polite">
      <span className="sr-only">در حال بارگذاری...</span>
      {Array.from({ length: 4 }).map((_, index) => (
        <div key={index} className="adm-skeleton h-10 w-full" />
      ))}
    </div>
  );
}
