"use client";

import type { AiProviderStatusDto } from "@/lib/admin/ai/ai-api";

type Props = {
  provider: AiProviderStatusDto | null | undefined;
  successRate: number;
  averageLatencyMs: number;
};

export function AiHealthCard({ provider, successRate, averageLatencyMs }: Props) {
  const status = provider?.healthStatus ?? "—";
  return (
    <div
      className="rounded-[var(--admin-radius)] border border-[var(--admin-border)] bg-[var(--admin-surface)] p-4"
      dir="rtl"
    >
      <p className="text-xs text-[var(--admin-muted)]">سلامت پلتفرم AI</p>
      <p className="mt-1 text-lg font-semibold">{status}</p>
      <dl className="mt-3 grid gap-2 text-sm sm:grid-cols-2">
        <div>
          <dt className="text-[var(--admin-muted)]">نرخ موفقیت امروز</dt>
          <dd>{(successRate * 100).toFixed(1)}٪</dd>
        </div>
        <div>
          <dt className="text-[var(--admin-muted)]">میانگین تأخیر</dt>
          <dd>{Math.round(averageLatencyMs)} ms</dd>
        </div>
      </dl>
    </div>
  );
}
