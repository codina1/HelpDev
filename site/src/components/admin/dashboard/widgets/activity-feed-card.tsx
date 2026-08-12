import { formatRelativeTimeFa } from "@/lib/admin/dashboard/dashboard-mappers";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import type { ActivityItem } from "@/lib/admin/dashboard/dashboard-types";
import type { AsyncSection } from "@/lib/admin/dashboard/dashboard-hooks";
import { WidgetCard } from "@/components/admin/dashboard/widgets/widget-card";
import Link from "next/link";

type ActivityFeedCardProps = {
  activity: AsyncSection<ActivityItem[]>;
  onRetry: () => void;
};

/**
 * Section 3 (left) — recent activity sourced from the Audit API. Only actor,
 * a friendly action label and time are shown; no metadata, payloads or secrets.
 */
export function ActivityFeedCard({ activity, onRetry }: ActivityFeedCardProps) {
  const items = activity.data ?? [];

  return (
    <WidgetCard
      title="فعالیت‌های اخیر"
      icon="activity"
      headerAction={
        <Link
          href={ADMIN_ROUTES.audit}
          className="text-[12px] font-semibold text-[var(--adm-accent-text)] hover:underline"
        >
          مشاهده همه
        </Link>
      }
      loading={activity.loading}
      error={activity.error}
      isEmpty={!activity.loading && !activity.error && items.length === 0}
      emptyTitle="هنوز فعالیتی ثبت نشده است"
      emptyIcon="activity"
      onRetry={onRetry}
      className="h-full"
    >
      <ul className="space-y-1">
        {items.map((item) => {
          const failed = item.outcome.toLowerCase() !== "success";
          return (
            <li
              key={item.id}
              className="flex items-start gap-3 rounded-lg px-2 py-2 hover:bg-[var(--adm-surface-2)]"
            >
              <span
                aria-hidden
                className={`mt-1.5 h-2 w-2 shrink-0 rounded-full ${
                  failed ? "bg-[var(--adm-danger)]" : "bg-[var(--adm-success)]"
                }`}
              />
              <div className="min-w-0 flex-1">
                <p className="adm-text text-[13px] leading-6">
                  <span className="font-bold">{item.actorLabel}</span>{" "}
                  <span className="adm-muted">{item.actionLabel}</span>
                </p>
                <time
                  className="adm-subtle text-[11px]"
                  dateTime={item.occurredAtUtc}
                >
                  {formatRelativeTimeFa(item.occurredAtUtc)}
                </time>
              </div>
            </li>
          );
        })}
      </ul>
    </WidgetCard>
  );
}
