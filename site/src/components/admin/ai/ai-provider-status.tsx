"use client";

import type { AiProviderStatusDto } from "@/lib/admin/ai/ai-api";

type Props = {
  provider: AiProviderStatusDto | null | undefined;
};

function statusTone(status: string | undefined): string {
  switch (status) {
    case "Healthy":
      return "var(--admin-success)";
    case "Degraded":
      return "var(--admin-warning)";
    case "Unhealthy":
      return "var(--admin-danger)";
    default:
      return "var(--admin-muted)";
  }
}

export function AiProviderStatus({ provider }: Props) {
  if (!provider) {
    return <p className="text-sm text-[var(--admin-muted)]">وضعیت ارائه‌دهنده در دسترس نیست.</p>;
  }

  const lastSuccess = provider.lastSuccessfulCallAtUtc
    ? new Date(provider.lastSuccessfulCallAtUtc).toLocaleString("fa-IR")
    : "—";

  return (
    <div className="space-y-2 text-sm" dir="rtl">
      <div className="flex items-center justify-between gap-3">
        <span className="text-[var(--admin-muted)]">ارائه‌دهنده</span>
        <span className="font-medium">{provider.name}</span>
      </div>
      <div className="flex items-center justify-between gap-3">
        <span className="text-[var(--admin-muted)]">پیکربندی</span>
        <span>{provider.configured ? "فعال" : "ناقص"}</span>
      </div>
      <div className="flex items-center justify-between gap-3">
        <span className="text-[var(--admin-muted)]">سلامت</span>
        <span style={{ color: statusTone(provider.healthStatus) }}>{provider.healthStatus}</span>
      </div>
      <div className="flex items-center justify-between gap-3">
        <span className="text-[var(--admin-muted)]">آخرین موفقیت</span>
        <span>{lastSuccess}</span>
      </div>
    </div>
  );
}
