"use client";

import { useContentAnalyticsDashboard } from "@/lib/admin/analytics/content/content-analytics-hooks";
import { MetricCard } from "@/components/admin/analytics/content/metric-card";
import { TopContentTable } from "@/components/admin/analytics/content/top-content-table";
import { ContentHealthPanel } from "@/components/admin/analytics/content/content-health-panel";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { AdminPageSection } from "@/components/admin/page/admin-page-section";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";

export function ContentAnalyticsDashboard() {
  const { overview, top, health } = useContentAnalyticsDashboard();

  const loading = overview.loading || top.loading || health.loading;
  const firstError = overview.error ?? top.error ?? health.error;

  if (loading && !overview.data) {
    return (
      <div className="space-y-6">
        <AdminPageHeader
          title="تحلیل محتوا"
          description="متریک‌های واقعی از analytics_daily_metrics — بدون تخمین ترافیک"
        />
        <AdminLoadingState cards={4} rows={4} />
      </div>
    );
  }

  if (firstError && !overview.data) {
    return (
      <div className="space-y-6">
        <AdminPageHeader
          title="تحلیل محتوا"
          description="متریک‌های واقعی از analytics_daily_metrics — بدون تخمین ترافیک"
        />
        <AdminErrorState
          error={firstError}
          title="بارگذاری تحلیل محتوا ناموفق بود"
          onRetry={() => {
            overview.reload();
            top.reload();
            health.reload();
          }}
        />
      </div>
    );
  }

  const data = overview.data;

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="تحلیل محتوا"
        description="بازدیدها از رویداد content.item_viewed؛ Favorite/Share هنوز تولید نمی‌شوند."
      />

      <AdminPageSection title="پوشش محتوا">
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <MetricCard label="بازدید کل (بازه)" value={data?.totalViews ?? 0} icon="analytics" tone="info" />
          <MetricCard label="ایجادشده" value={data?.contentCreated ?? 0} icon="content" />
          <MetricCard label="منتشرشده" value={data?.contentPublished ?? 0} icon="check" tone="success" />
          <MetricCard
            label="محتوا با بازدید"
            value={data?.contentsWithViews ?? 0}
            icon="flag"
            tone="warning"
          />
        </div>
      </AdminPageSection>

      <AdminPageSection title="پربازدیدترین محتوا">
        {top.error ? (
          <AdminErrorState error={top.error} title="خطا در فهرست برتر" onRetry={top.reload} showHome={false} />
        ) : (
          <TopContentTable items={top.data ?? []} />
        )}
      </AdminPageSection>

      <AdminPageSection title="سلامت محتوا">
        {health.error ? (
          <AdminErrorState error={health.error} title="خطا در سلامت محتوا" onRetry={health.reload} showHome={false} />
        ) : (
          <ContentHealthPanel items={health.data ?? []} />
        )}
      </AdminPageSection>
    </div>
  );
}
