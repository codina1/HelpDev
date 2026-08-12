import type { AdminNavTone } from "@/lib/admin/navigation";
import { getAdminEnvironmentMeta } from "@/lib/admin/environment";

const TONE_CLASS: Record<AdminNavTone, string> = {
  neutral: "bg-[var(--adm-surface-3)] text-[var(--adm-text-muted)]",
  info: "bg-[var(--adm-info-soft)] text-[var(--adm-info)]",
  success: "bg-[var(--adm-success-soft)] text-[var(--adm-success)]",
  warning: "bg-[var(--adm-warning-soft)] text-[var(--adm-warning)]",
  danger: "bg-[var(--adm-danger-soft)] text-[var(--adm-danger)] font-bold",
};

/**
 * Shows the deployment environment sourced from safe public config. Production
 * is visually high-attention. Contains no secrets.
 */
export function EnvironmentBadge() {
  const meta = getAdminEnvironmentMeta();

  return (
    <span
      title={meta.description}
      className={`inline-flex items-center gap-1.5 rounded-md px-2 py-1 text-[11px] font-semibold ${TONE_CLASS[meta.tone]}`}
    >
      <span
        aria-hidden
        className="h-1.5 w-1.5 rounded-full bg-current"
      />
      <span className="hidden sm:inline">{meta.label}</span>
      <span className="sr-only">محیط: {meta.label}</span>
    </span>
  );
}
