"use client";

import { useContentItemAnalytics } from "@/lib/admin/analytics/content/content-analytics-hooks";
import { ContentPerformanceCard } from "@/components/admin/analytics/content/content-performance-card";
import { ContentHealthPanel } from "@/components/admin/analytics/content/content-health-panel";
import { ContentDetailTabs } from "@/components/admin/content/details/content-detail-tabs";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { AdminPageSection } from "@/components/admin/page/admin-page-section";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";

export function ContentItemAnalyticsWorkspace({ contentId }: { contentId: string }) {
  const { data, loading, error, reload } = useContentItemAnalytics(contentId);

  return (
    <div className="space-y-6">
      <AdminPageHeader title="تحلیل محتوا" description="متریک‌ها و سلامت برای این آیتم" />
      <ContentDetailTabs id={contentId} active="analytics" />

      {loading && !data ? <AdminLoadingState cards={2} rows={3} /> : null}

      {error ? (
        <AdminErrorState error={error} title="بارگذاری تحلیل ناموفق بود" onRetry={reload} />
      ) : null}

      {!loading && !error && data ? (
        <>
          <AdminPageSection title="عملکرد">
            <ContentPerformanceCard performance={data.performance} />
          </AdminPageSection>
          <AdminPageSection title="سلامت">
            {data.health ? (
              <ContentHealthPanel items={[data.health]} />
            ) : (
              <p className="adm-subtle text-[13px]">شاخص سلامت برای این محتوا در دسترس نیست.</p>
            )}
          </AdminPageSection>
        </>
      ) : null}

      {!loading && !error && !data ? (
        <p className="adm-subtle text-center text-[13px]">داده‌ای برای نمایش وجود ندارد.</p>
      ) : null}
    </div>
  );
}
