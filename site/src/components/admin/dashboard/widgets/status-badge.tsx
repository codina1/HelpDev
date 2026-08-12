import type { OperationalStatus } from "@/lib/admin/dashboard/dashboard-types";

const STATUS_META: Record<
  OperationalStatus,
  { label: string; className: string }
> = {
  Healthy: {
    label: "سالم",
    className: "bg-[var(--adm-success-soft)] text-[var(--adm-success)]",
  },
  Degraded: {
    label: "نیاز به بررسی",
    className: "bg-[var(--adm-warning-soft)] text-[var(--adm-warning)]",
  },
  Unhealthy: {
    label: "خطا",
    className: "bg-[var(--adm-danger-soft)] text-[var(--adm-danger)]",
  },
  Unknown: {
    label: "نامشخص",
    className: "bg-[var(--adm-surface-3)] text-[var(--adm-text-muted)]",
  },
};

export function statusLabel(status: OperationalStatus): string {
  return STATUS_META[status].label;
}

export function StatusBadge({ status }: { status: OperationalStatus }) {
  const meta = STATUS_META[status];
  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-md px-2 py-0.5 text-[11px] font-bold ${meta.className}`}
    >
      <span aria-hidden className="h-1.5 w-1.5 rounded-full bg-current" />
      {meta.label}
    </span>
  );
}
