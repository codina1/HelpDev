import type { AdminNavTone } from "@/lib/admin/navigation";
import { formatNumberFa } from "@/lib/admin/dashboard/dashboard-mappers";
import type {
  DashboardOverview,
  OperationalStatus,
  SystemHealth,
} from "@/lib/admin/dashboard/dashboard-types";
import type { AsyncSection } from "@/lib/admin/dashboard/dashboard-hooks";
import { KpiCard } from "@/components/admin/dashboard/widgets/kpi-card";
import { statusLabel } from "@/components/admin/dashboard/widgets/status-badge";

type KpiGridProps = {
  overview: AsyncSection<DashboardOverview>;
  health: AsyncSection<SystemHealth>;
  onRetry: () => void;
};

const STATUS_TONE: Record<OperationalStatus, AdminNavTone> = {
  Healthy: "success",
  Degraded: "warning",
  Unhealthy: "danger",
  Unknown: "neutral",
};

/** Section 1 — KPI overview: Users, Content, Courses, System. */
export function KpiGrid({ overview, health, onRetry }: KpiGridProps) {
  const o = overview.data;
  const h = health.data;

  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
      <KpiCard
        label="کاربران"
        icon="users"
        tone="info"
        value={o ? formatNumberFa(o.users.total) : "—"}
        subtitle={o ? `${formatNumberFa(o.users.active)} کاربر فعال` : undefined}
        loading={overview.loading}
        error={overview.error}
        onRetry={onRetry}
      />
      <KpiCard
        label="محتوا"
        icon="content"
        tone="success"
        value={o ? formatNumberFa(o.content.total) : "—"}
        subtitle={
          o
            ? `${formatNumberFa(o.content.published)} منتشرشده · ${formatNumberFa(o.content.draft)} پیش‌نویس`
            : undefined
        }
        loading={overview.loading}
        error={overview.error}
        onRetry={onRetry}
      />
      <KpiCard
        label="دوره‌ها"
        icon="learning"
        tone="warning"
        value={o ? formatNumberFa(o.learning.courses) : "—"}
        subtitle={o ? `${formatNumberFa(o.learning.enrollments)} ثبت‌نام` : undefined}
        loading={overview.loading}
        error={overview.error}
        onRetry={onRetry}
      />
      <KpiCard
        label="سیستم"
        icon="health"
        tone={h ? STATUS_TONE[h.overall] : "neutral"}
        value={h ? statusLabel(h.overall) : "—"}
        subtitle={h ? `${formatNumberFa(h.healthyCount)} از ${formatNumberFa(h.totalCount)} سرویس سالم` : undefined}
        loading={health.loading}
        error={health.error}
        onRetry={onRetry}
      />
    </div>
  );
}
