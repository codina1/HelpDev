import Link from "next/link";
import type { AdminNavBadge, AdminNavItem as AdminNavItemModel, AdminNavTone } from "@/lib/admin/navigation";
import { AdminIcon } from "@/components/admin/shared/admin-icons";

type AdminNavItemProps = {
  item: AdminNavItemModel;
  active: boolean;
  /** When true (mobile drawer), always show labels regardless of collapse. */
  showLabel?: boolean;
  onNavigate?: () => void;
};

const BADGE_TONE: Record<AdminNavTone, string> = {
  neutral: "bg-[var(--adm-surface-3)] text-[var(--adm-text-muted)]",
  info: "bg-[var(--adm-info-soft)] text-[var(--adm-info)]",
  success: "bg-[var(--adm-success-soft)] text-[var(--adm-success)]",
  warning: "bg-[var(--adm-warning-soft)] text-[var(--adm-warning)]",
  danger: "bg-[var(--adm-danger-soft)] text-[var(--adm-danger)]",
};

function Badge({ badge }: { badge: AdminNavBadge }) {
  return (
    <span
      className={`adm-collapsible ms-auto rounded-md px-1.5 py-0.5 text-[10px] font-bold ${
        BADGE_TONE[badge.tone ?? "neutral"]
      }`}
    >
      {badge.value}
    </span>
  );
}

/**
 * A single sidebar navigation entry. `ready` items are links; `future` items
 * render as a clearly-labelled disabled row (no navigation, no 404).
 */
export function AdminNavItem({
  item,
  active,
  showLabel = false,
  onNavigate,
}: AdminNavItemProps) {
  const label = (
    <span className={showLabel ? "truncate" : "adm-nav-label truncate"}>
      {item.title}
    </span>
  );

  if (item.status === "future" || !item.href) {
    return (
      <span
        title={`${item.title} — به‌زودی`}
        aria-disabled="true"
        className="adm-nav-item cursor-not-allowed opacity-55"
      >
        <AdminIcon name={item.icon} size={18} className="shrink-0" />
        {label}
        <span className="adm-collapsible ms-auto rounded-md bg-[var(--adm-surface-3)] px-1.5 py-0.5 text-[9px] font-bold text-[var(--adm-text-subtle)]">
          به‌زودی
        </span>
      </span>
    );
  }

  return (
    <Link
      href={item.href}
      title={item.title}
      onClick={onNavigate}
      aria-current={active ? "page" : undefined}
      className={`adm-nav-item adm-focus ${active ? "adm-nav-item-active" : ""}`}
    >
      <AdminIcon name={item.icon} size={18} className="shrink-0" />
      {label}
      {item.badge ? <Badge badge={item.badge} /> : null}
    </Link>
  );
}
