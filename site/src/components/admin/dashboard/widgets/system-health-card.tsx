import type { AdminIconName } from "@/lib/admin/navigation";
import type { SystemHealth } from "@/lib/admin/dashboard/dashboard-types";
import type { AsyncSection } from "@/lib/admin/dashboard/dashboard-hooks";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { WidgetCard } from "@/components/admin/dashboard/widgets/widget-card";
import {
  StatusBadge,
  statusLabel,
} from "@/components/admin/dashboard/widgets/status-badge";

type SystemHealthCardProps = {
  health: AsyncSection<SystemHealth>;
  onRetry: () => void;
};

const COMPONENT_ICONS: Record<string, AdminIconName> = {
  api: "activity",
  database: "settings",
  search: "search",
  outbox: "outbox",
  analytics: "analytics",
  audit: "audit",
};

/** Section 2 (right) — subsystem health from operations/health. */
export function SystemHealthCard({ health, onRetry }: SystemHealthCardProps) {
  const data = health.data;

  return (
    <WidgetCard
      title="سلامت سیستم"
      icon="health"
      headerAction={data ? <StatusBadge status={data.overall} /> : undefined}
      loading={health.loading}
      error={health.error}
      isEmpty={!health.loading && !health.error && (data?.components.length ?? 0) === 0}
      emptyTitle="اطلاعات وضعیت در دسترس نیست"
      emptyIcon="health"
      onRetry={onRetry}
      className="h-full"
    >
      {data ? (
        <div className="space-y-4">
          <p className="adm-muted text-[12px]">
            {`${statusLabel(data.overall)} · محیط ${data.environment} · نسخه ${data.version}`}
          </p>
          <ul className="grid grid-cols-1 gap-2 sm:grid-cols-2">
            {data.components.map((component) => (
              <li
                key={component.key}
                className="flex items-center justify-between gap-2 rounded-lg bg-[var(--adm-surface-2)] px-3 py-2"
              >
                <span className="flex min-w-0 items-center gap-2">
                  <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-md bg-[var(--adm-surface-3)] text-[var(--adm-text-muted)]">
                    <AdminIcon name={COMPONENT_ICONS[component.key] ?? "health"} size={15} />
                  </span>
                  <span className="adm-text truncate text-[13px] font-medium">
                    {component.label}
                  </span>
                </span>
                <StatusBadge status={component.status} />
              </li>
            ))}
          </ul>
        </div>
      ) : null}
    </WidgetCard>
  );
}
