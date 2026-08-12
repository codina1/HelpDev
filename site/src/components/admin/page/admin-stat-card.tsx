import type { AdminIconName, AdminNavTone } from "@/lib/admin/navigation";
import { AdminIcon } from "@/components/admin/shared/admin-icons";

type Trend = {
  direction: "up" | "down" | "flat";
  label: string;
};

type AdminStatCardProps = {
  label: string;
  value: string | number;
  icon?: AdminIconName;
  tone?: AdminNavTone;
  description?: string;
  trend?: Trend;
  loading?: boolean;
};

const TONE_BG: Record<AdminNavTone, string> = {
  neutral: "bg-[var(--adm-surface-3)] text-[var(--adm-text-muted)]",
  info: "bg-[var(--adm-info-soft)] text-[var(--adm-info)]",
  success: "bg-[var(--adm-success-soft)] text-[var(--adm-success)]",
  warning: "bg-[var(--adm-warning-soft)] text-[var(--adm-warning)]",
  danger: "bg-[var(--adm-danger-soft)] text-[var(--adm-danger)]",
};

const TREND_COLOR: Record<Trend["direction"], string> = {
  up: "text-[var(--adm-success)]",
  down: "text-[var(--adm-danger)]",
  flat: "text-[var(--adm-text-subtle)]",
};

export function AdminStatCard({
  label,
  value,
  icon,
  tone = "neutral",
  description,
  trend,
  loading = false,
}: AdminStatCardProps) {
  return (
    <div className="adm-surface rounded-xl p-4">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0 space-y-1">
          <p className="adm-muted text-[12px] font-medium">{label}</p>
          {loading ? (
            <div className="adm-skeleton h-7 w-20" />
          ) : (
            <p className="adm-text text-2xl font-black tabular-nums">{value}</p>
          )}
          {description ? (
            <p className="adm-subtle text-[11px]">{description}</p>
          ) : null}
        </div>
        {icon ? (
          <span
            className={`flex h-9 w-9 shrink-0 items-center justify-center rounded-lg ${TONE_BG[tone]}`}
          >
            <AdminIcon name={icon} size={18} />
          </span>
        ) : null}
      </div>
      {trend && !loading ? (
        <p className={`mt-2 text-[11px] font-semibold ${TREND_COLOR[trend.direction]}`}>
          {trend.label}
        </p>
      ) : null}
    </div>
  );
}
