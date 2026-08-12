import Link from "next/link";
import { formatRelativeTimeFa } from "@/lib/admin/dashboard/dashboard-mappers";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import type { RecentContentItem } from "@/lib/admin/dashboard/dashboard-types";
import type { AsyncSection } from "@/lib/admin/dashboard/dashboard-hooks";
import { WidgetCard } from "@/components/admin/dashboard/widgets/widget-card";

type RecentContentCardProps = {
  recentContent: AsyncSection<RecentContentItem[]>;
  onRetry: () => void;
};

/** Section 4 — recent content, compact and responsive (no horizontal scroll). */
export function RecentContentCard({ recentContent, onRetry }: RecentContentCardProps) {
  const items = recentContent.data ?? [];

  return (
    <WidgetCard
      title="محتوای اخیر"
      icon="news"
      headerAction={
        <Link
          href={ADMIN_ROUTES.content}
          className="text-[12px] font-semibold text-[var(--adm-accent-text)] hover:underline"
        >
          مشاهده همه
        </Link>
      }
      loading={recentContent.loading}
      error={recentContent.error}
      isEmpty={!recentContent.loading && !recentContent.error && items.length === 0}
      emptyTitle="محتوایی وجود ندارد"
      emptyIcon="content"
      onRetry={onRetry}
    >
      <ul className="divide-y divide-[var(--adm-border)]">
        {items.map((item) => (
          <li
            key={item.id}
            className="grid grid-cols-[1fr_auto] items-center gap-x-3 gap-y-1 py-2.5"
          >
            <span className="adm-text truncate text-[13px] font-semibold" title={item.title}>
              {item.title}
            </span>
            <StatusChip status={item.status} />
            <span className="adm-subtle col-span-2 flex items-center gap-2 text-[11px]">
              <span className="rounded bg-[var(--adm-surface-3)] px-1.5 py-0.5 font-medium text-[var(--adm-text-muted)]">
                {item.typeLabel}
              </span>
              <time dateTime={item.createdAt}>{formatRelativeTimeFa(item.createdAt)}</time>
            </span>
          </li>
        ))}
      </ul>
    </WidgetCard>
  );
}

function StatusChip({ status }: { status: string }) {
  const isDraft = status.includes("پیش") || status.toLowerCase() === "draft";
  const cls = isDraft
    ? "bg-[var(--adm-warning-soft)] text-[var(--adm-warning)]"
    : "bg-[var(--adm-success-soft)] text-[var(--adm-success)]";
  const label = isDraft ? "پیش‌نویس" : "منتشرشده";
  return (
    <span className={`justify-self-end rounded-md px-2 py-0.5 text-[11px] font-bold ${cls}`}>
      {label}
    </span>
  );
}
