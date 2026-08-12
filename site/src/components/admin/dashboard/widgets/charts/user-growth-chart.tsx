import type { DashboardOverview } from "@/lib/admin/dashboard/dashboard-types";
import type { AsyncSection } from "@/lib/admin/dashboard/dashboard-hooks";
import { WidgetCard } from "@/components/admin/dashboard/widgets/widget-card";
import { DashboardBarChart } from "@/components/admin/dashboard/widgets/charts/dashboard-bar-chart";

type UserGrowthChartProps = {
  overview: AsyncSection<DashboardOverview>;
  onRetry: () => void;
};

/**
 * User composition chart built from real snapshot values. A true growth
 * (time-series) chart requires a historical endpoint that the backend does not
 * expose yet (see docs/admin/admin-dashboard.md → Future widgets).
 */
export function UserGrowthChart({ overview, onRetry }: UserGrowthChartProps) {
  const data = overview.data;

  return (
    <WidgetCard
      title="ترکیب کاربران"
      icon="analytics"
      loading={overview.loading}
      error={overview.error}
      isEmpty={!overview.loading && !overview.error && (data?.users.total ?? 0) === 0}
      emptyTitle="داده‌ای برای نمودار نیست"
      emptyIcon="users"
      onRetry={onRetry}
      className="h-full"
    >
      {data ? (
        <DashboardBarChart
          ariaLabel="ترکیب کاربران: کل، فعال، عضویت امروز"
          colorVar="--adm-accent"
          data={[
            { label: "کل", value: data.users.total },
            { label: "فعال", value: data.users.active },
            { label: "امروز", value: data.users.registrationsToday },
          ]}
        />
      ) : null}
    </WidgetCard>
  );
}
