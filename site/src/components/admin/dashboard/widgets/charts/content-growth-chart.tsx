import type { ContentPipeline } from "@/lib/admin/dashboard/dashboard-types";
import type { AsyncSection } from "@/lib/admin/dashboard/dashboard-hooks";
import { WidgetCard } from "@/components/admin/dashboard/widgets/widget-card";
import { DashboardBarChart } from "@/components/admin/dashboard/widgets/charts/dashboard-bar-chart";

type ContentGrowthChartProps = {
  pipeline: AsyncSection<ContentPipeline>;
  onRetry: () => void;
};

/**
 * Content composition chart built from real snapshot values. A true growth
 * (time-series) chart requires a historical endpoint that the backend does not
 * expose yet (see docs/admin/admin-dashboard.md → Future widgets).
 */
export function ContentGrowthChart({ pipeline, onRetry }: ContentGrowthChartProps) {
  const data = pipeline.data;

  return (
    <WidgetCard
      title="ترکیب محتوا"
      icon="analytics"
      loading={pipeline.loading}
      error={pipeline.error}
      isEmpty={!pipeline.loading && !pipeline.error && (data?.total ?? 0) === 0}
      emptyTitle="داده‌ای برای نمودار نیست"
      emptyIcon="content"
      onRetry={onRetry}
      className="h-full"
    >
      {data ? (
        <DashboardBarChart
          ariaLabel="ترکیب محتوا: کل، منتشرشده، پیش‌نویس"
          colorVar="--adm-success"
          data={[
            { label: "کل", value: data.total },
            { label: "منتشرشده", value: data.published },
            { label: "پیش‌نویس", value: data.draft },
          ]}
        />
      ) : null}
    </WidgetCard>
  );
}
