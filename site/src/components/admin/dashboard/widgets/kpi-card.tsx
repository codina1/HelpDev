import type { AdminIconName, AdminNavTone } from "@/lib/admin/navigation";
import { AdminStatCard } from "@/components/admin/page/admin-stat-card";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { ApiClientError } from "@/lib/api/errors";

type Trend = { direction: "up" | "down" | "flat"; label: string };

type KpiCardProps = {
  label: string;
  value: string | number;
  subtitle?: string;
  icon?: AdminIconName;
  tone?: AdminNavTone;
  trend?: Trend;
  loading?: boolean;
  error?: unknown;
  onRetry?: () => void;
};

/**
 * KPI tile built on top of the shared AdminStatCard. Adds a compact, safe error
 * state so a single failing metric never blanks the whole KPI row.
 */
export function KpiCard({
  label,
  value,
  subtitle,
  icon,
  tone = "neutral",
  trend,
  loading = false,
  error = null,
  onRetry,
}: KpiCardProps) {
  if (error) {
    return (
      <div className="adm-surface rounded-xl p-4">
        <div className="flex items-start justify-between gap-3">
          <div className="min-w-0 space-y-1">
            <p className="adm-muted text-[12px] font-medium">{label}</p>
            <p className="text-[13px] font-bold text-[var(--adm-danger)]">
              {toShortMessage(error)}
            </p>
          </div>
          <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-[var(--adm-danger-soft)] text-[var(--adm-danger)]">
            <AdminIcon name="health" size={18} />
          </span>
        </div>
        {onRetry ? (
          <button
            type="button"
            onClick={onRetry}
            className="adm-btn adm-btn-outline adm-focus mt-3 text-[11px]"
          >
            تلاش مجدد
          </button>
        ) : null}
      </div>
    );
  }

  return (
    <AdminStatCard
      label={label}
      value={value}
      icon={icon}
      tone={tone}
      description={subtitle}
      trend={trend}
      loading={loading}
    />
  );
}

function toShortMessage(error: unknown): string {
  if (error instanceof ApiClientError) {
    if (error.isNetworkError) return "اتصال برقرار نشد";
    if (error.isForbidden) return "دسترسی کافی نیست";
    if (error.isServerError) return "خطای سرور";
  }
  return "خطا در دریافت";
}
