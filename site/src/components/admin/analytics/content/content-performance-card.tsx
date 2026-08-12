"use client";

import { MetricCard } from "@/components/admin/analytics/content/metric-card";
import type { ContentPerformanceDto } from "@/lib/admin/analytics/content/content-analytics-types";
import { formatDateTimeFa } from "@/lib/admin/content/content-mappers";
import { AdminSurface } from "@/components/admin/page/admin-surface";

type ContentPerformanceCardProps = {
  performance: ContentPerformanceDto | null;
};

export function ContentPerformanceCard({ performance }: ContentPerformanceCardProps) {
  if (!performance) {
    return (
      <AdminSurface className="p-4">
        <p className="adm-subtle text-[13px]">برای این محتوا متریک ذخیره‌شده‌ای وجود ندارد.</p>
      </AdminSurface>
    );
  }

  return (
    <AdminSurface className="space-y-4 p-4">
      <div>
        <h3 className="adm-text text-[14px] font-bold">{performance.title}</h3>
        <p className="adm-subtle text-[11px]">
          زمان تولید گزارش: {formatDateTimeFa(performance.generatedAtUtc)}
        </p>
      </div>
      <div className="grid gap-3 sm:grid-cols-2">
        <MetricCard label="بازدید (بازه)" value={performance.views} icon="analytics" tone="info" />
        {performance.metrics.map((metric) => (
          <MetricCard
            key={metric.metricType}
            label={metric.metricType === "View" ? "بازدید" : metric.metricType}
            value={metric.value}
            description={`${metric.periodStartUtc} → ${metric.periodEndUtc}`}
          />
        ))}
      </div>
      <p className="adm-subtle text-[11px]">
        متریک‌های Favorite / Share / Save در v1 تولید نمی‌شوند مگر آنکه رویداد واقعی اضافه شود.
      </p>
    </AdminSurface>
  );
}
