"use client";

import Link from "next/link";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { formatDateTimeFa } from "@/lib/admin/content/content-mappers";
import type { ContentHealthIndicatorDto } from "@/lib/admin/analytics/content/content-analytics-types";
import { AdminSurface } from "@/components/admin/page/admin-surface";

type ContentHealthPanelProps = {
  items: ContentHealthIndicatorDto[];
};

const STATUS_LABEL: Record<string, string> = {
  Healthy: "سالم",
  NeedsAttention: "نیاز به توجه",
  Critical: "بحرانی",
  Unknown: "نامشخص",
};

export function ContentHealthPanel({ items }: ContentHealthPanelProps) {
  if (items.length === 0) {
    return <p className="adm-subtle text-[13px]">محتوایی برای ارزیابی سلامت یافت نشد.</p>;
  }

  return (
    <ul className="space-y-3">
      {items.map((item) => (
        <li key={item.contentId}>
          <AdminSurface className="space-y-2 p-3">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <Link
                href={`${ADMIN_ROUTES.content}/${encodeURIComponent(item.contentId)}/analytics`}
                className="adm-link text-[13px] font-bold"
              >
                {item.title}
              </Link>
              <span className="adm-subtle text-[11px] font-semibold">
                {STATUS_LABEL[item.healthStatus] ?? item.healthStatus}
              </span>
            </div>
            <p className="adm-subtle text-[11px]">
              به‌روزرسانی: {formatDateTimeFa(item.updatedAtUtc)} · بازنویسی‌ها: {item.revisionCount}
              {item.viewsInPeriod != null ? ` · بازدید بازه: ${item.viewsInPeriod}` : ""}
            </p>
            {item.reasons.length > 0 ? (
              <ul className="list-inside list-disc space-y-0.5 text-[12px] text-[var(--adm-warning)]">
                {item.reasons.map((reason) => (
                  <li key={reason}>{reason}</li>
                ))}
              </ul>
            ) : (
              <p className="text-[12px] text-[var(--adm-success)]">عامل هشداری یافت نشد.</p>
            )}
          </AdminSurface>
        </li>
      ))}
    </ul>
  );
}
