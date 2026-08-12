"use client";

import dynamic from "next/dynamic";
import { useAdminDashboard } from "@/lib/admin/dashboard/dashboard-hooks";
import { AdminPageSection } from "@/components/admin/page/admin-page-section";
import { DashboardHeader } from "@/components/admin/dashboard/widgets/dashboard-header";
import { KpiGrid } from "@/components/admin/dashboard/widgets/kpi-grid";
import { ContentPipelineCard } from "@/components/admin/dashboard/widgets/content-pipeline-card";
import { SystemHealthCard } from "@/components/admin/dashboard/widgets/system-health-card";
import { OperationsSummaryCard } from "@/components/admin/dashboard/widgets/operations-summary-card";
import { ModuleStatusCard } from "@/components/admin/dashboard/widgets/module-status-card";
import { ActivityFeedCard } from "@/components/admin/dashboard/widgets/activity-feed-card";
import { QuickActionsCard } from "@/components/admin/dashboard/widgets/quick-actions-card";
import { RecentContentCard } from "@/components/admin/dashboard/widgets/recent-content-card";
import { WidgetChartSkeleton } from "@/components/admin/dashboard/widgets/dashboard-skeleton";

// Charts are lazy-loaded (client-only) to keep the initial dashboard fast.
const UserGrowthChart = dynamic(
  () =>
    import("@/components/admin/dashboard/widgets/charts/user-growth-chart").then(
      (m) => m.UserGrowthChart,
    ),
  { ssr: false, loading: () => <WidgetChartSkeleton /> },
);

const ContentGrowthChart = dynamic(
  () =>
    import("@/components/admin/dashboard/widgets/charts/content-growth-chart").then(
      (m) => m.ContentGrowthChart,
    ),
  { ssr: false, loading: () => <WidgetChartSkeleton /> },
);

/**
 * HelpDev Admin Command Center.
 *
 * Pure composition: all data access lives in `lib/admin/dashboard` and every
 * widget owns its Loading / Error / Empty / Success states. No metric is
 * hardcoded — values come exclusively from existing `/api/v1` endpoints.
 */
export function AdminDashboard() {
  const {
    overview,
    pipeline,
    health,
    operations,
    activity,
    recentContent,
    reload,
  } = useAdminDashboard();

  return (
    <div className="space-y-6">
      <DashboardHeader />

      <AdminPageSection title="نمای کلی">
        <KpiGrid overview={overview} health={health} onRetry={reload} />
      </AdminPageSection>

      <AdminPageSection title="عملیات اصلی">
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
          <ContentPipelineCard pipeline={pipeline} onRetry={reload} />
          <SystemHealthCard health={health} onRetry={reload} />
        </div>
      </AdminPageSection>

      <AdminPageSection title="عملیات و ماژول‌ها">
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
          <OperationsSummaryCard operations={operations} onRetry={reload} />
          <ModuleStatusCard />
        </div>
      </AdminPageSection>

      <AdminPageSection title="نمای تحلیلی">
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
          <UserGrowthChart overview={overview} onRetry={reload} />
          <ContentGrowthChart pipeline={pipeline} onRetry={reload} />
        </div>
      </AdminPageSection>

      <AdminPageSection title="فعالیت و اقدامات">
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
          <ActivityFeedCard activity={activity} onRetry={reload} />
          <QuickActionsCard />
        </div>
      </AdminPageSection>

      <AdminPageSection title="محتوای اخیر">
        <RecentContentCard recentContent={recentContent} onRetry={reload} />
      </AdminPageSection>
    </div>
  );
}
