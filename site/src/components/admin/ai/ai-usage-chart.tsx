"use client";

import type { AiUsagePointDto } from "@/lib/admin/ai/ai-api";

type Props = {
  points: AiUsagePointDto[];
};

export function AiUsageChart({ points }: Props) {
  if (points.length === 0) {
    return (
      <p className="text-sm text-[var(--admin-muted)]" dir="rtl">
        هنوز درخواست AI برای امروز ثبت نشده است.
      </p>
    );
  }

  const max = Math.max(...points.map((p) => p.requests), 1);

  return (
    <div className="space-y-3" dir="rtl">
      {points.map((point) => {
        const label = new Date(point.hourUtc).toLocaleTimeString("fa-IR", {
          hour: "2-digit",
          minute: "2-digit",
        });
        const width = `${Math.max(4, Math.round((point.requests / max) * 100))}%`;
        return (
          <div key={point.hourUtc} className="space-y-1">
            <div className="flex items-center justify-between text-xs text-[var(--admin-muted)]">
              <span>{label}</span>
              <span>
                {point.requests} · موفق {point.successes} · ناموفق {point.failures}
              </span>
            </div>
            <div className="h-2 overflow-hidden rounded bg-[var(--admin-surface-muted)]">
              <div
                className="h-full rounded bg-[var(--admin-accent)]"
                style={{ width }}
                title={`${point.requests} درخواست`}
              />
            </div>
          </div>
        );
      })}
    </div>
  );
}
