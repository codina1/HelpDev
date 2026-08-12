"use client";

import { useAiDashboard } from "@/lib/admin/ai/ai-hooks";
import { AiHealthCard } from "@/components/admin/ai/ai-health-card";
import { AiProviderStatus } from "@/components/admin/ai/ai-provider-status";
import { AiUsageChart } from "@/components/admin/ai/ai-usage-chart";
import { MetricCard } from "@/components/admin/analytics/content/metric-card";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { AdminPageSection } from "@/components/admin/page/admin-page-section";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";

export function AiDashboard() {
  const { data, loading, error, reload } = useAiDashboard();

  if (loading && !data) {
    return (
      <div className="space-y-6" dir="rtl">
        <AdminPageHeader
          title="عملیات AI"
          description="متریک‌های واقعی از ai_usage_records — بدون داده ساختگی"
        />
        <AdminLoadingState cards={4} rows={3} />
      </div>
    );
  }

  if (error && !data) {
    return (
      <div className="space-y-6" dir="rtl">
        <AdminPageHeader
          title="عملیات AI"
          description="متریک‌های واقعی از ai_usage_records — بدون داده ساختگی"
        />
        <AdminErrorState error={error} title="بارگذاری داشبورد AI ناموفق بود" onRetry={reload} />
      </div>
    );
  }

  return (
    <div className="space-y-6" dir="rtl">
      <AdminPageHeader
        title="عملیات AI"
        description="درخواست‌ها، نرخ موفقیت، تأخیر و وضعیت ارائه‌دهنده — بدون ذخیره پرامپت یا متن تولیدشده"
      />

      <AdminPageSection title="نمای امروز">
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <MetricCard label="درخواست‌های امروز" value={data?.requestsToday ?? 0} icon="activity" tone="info" />
          <MetricCard
            label="نرخ موفقیت (٪)"
            value={Math.round((data?.successRate ?? 0) * 1000) / 10}
            icon="check"
            tone="success"
          />
          <MetricCard
            label="میانگین تأخیر (ms)"
            value={Math.round(data?.averageLatencyMs ?? 0)}
            icon="analytics"
          />
          <MetricCard
            label="شکست‌ها"
            value={data?.failures.reduce((sum, f) => sum + f.count, 0) ?? 0}
            icon="flag"
            tone="warning"
          />
        </div>
      </AdminPageSection>

      <div className="grid gap-4 lg:grid-cols-2">
        <AdminPageSection title="وضعیت ارائه‌دهنده">
          <AiProviderStatus provider={data?.provider} />
        </AdminPageSection>
        <AiHealthCard
          provider={data?.provider}
          successRate={data?.successRate ?? 0}
          averageLatencyMs={data?.averageLatencyMs ?? 0}
        />
      </div>

      <AdminPageSection title="توزیع ساعتی امروز">
        <AiUsageChart points={data?.usageByHour ?? []} />
      </AdminPageSection>

      <AdminPageSection title="شکست‌ها بر اساس کد خطا">
        {(data?.failures.length ?? 0) === 0 ? (
          <p className="text-sm text-[var(--admin-muted)]">شکستی برای امروز ثبت نشده است.</p>
        ) : (
          <ul className="space-y-2 text-sm">
            {data!.failures.map((f) => (
              <li key={f.errorCode} className="flex items-center justify-between gap-3">
                <code className="text-[var(--admin-fg)]">{f.errorCode}</code>
                <span>{f.count}</span>
              </li>
            ))}
          </ul>
        )}
      </AdminPageSection>

      <AdminPageSection title="عملیات">
        {(data?.byOperation.length ?? 0) === 0 ? (
          <p className="text-sm text-[var(--admin-muted)]">عملیاتی ثبت نشده است.</p>
        ) : (
          <ul className="space-y-2 text-sm">
            {data!.byOperation.map((op) => (
              <li key={op.operation} className="flex items-center justify-between gap-3">
                <span>{op.operation}</span>
                <span>
                  {op.count} · موفق {op.successes}
                </span>
              </li>
            ))}
          </ul>
        )}
      </AdminPageSection>
    </div>
  );
}
