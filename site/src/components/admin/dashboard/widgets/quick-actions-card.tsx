import Link from "next/link";
import type { AdminIconName } from "@/lib/admin/navigation";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { WidgetCard } from "@/components/admin/dashboard/widgets/widget-card";

type QuickAction = {
  label: string;
  href: string;
  icon: AdminIconName;
  ready: boolean;
};

const ACTIONS: QuickAction[] = [
  { label: "ایجاد مقاله", href: ADMIN_ROUTES.contentNew, icon: "plus", ready: true },
  { label: "فهرست محتوا", href: ADMIN_ROUTES.content, icon: "content", ready: true },
  { label: "گردش‌کار محتوا", href: ADMIN_ROUTES.contentWorkflows, icon: "activity", ready: true },
  { label: "رسانه", href: ADMIN_ROUTES.media, icon: "media", ready: true },
  { label: "SEO", href: ADMIN_ROUTES.seo, icon: "seo", ready: true },
  { label: "عملیات سیستم", href: ADMIN_ROUTES.operations, icon: "health", ready: true },
  { label: "مدیریت کاربران", href: ADMIN_ROUTES.users, icon: "users", ready: true },
  { label: "مشاهده Audit", href: ADMIN_ROUTES.audit, icon: "audit", ready: true },
];

/** Section 3 (right) — quick actions. Only links to existing routes. */
export function QuickActionsCard() {
  return (
    <WidgetCard title="اقدامات سریع" icon="command" className="h-full">
      <ul className="grid grid-cols-1 gap-2 sm:grid-cols-2">
        {ACTIONS.map((action) => (
          <li key={action.label}>
            {action.ready ? (
              <Link
                href={action.href}
                className="adm-focus flex items-center gap-2.5 rounded-lg border border-[var(--adm-border)] bg-[var(--adm-surface-2)] px-3 py-2.5 transition-colors hover:border-[var(--adm-accent)] hover:bg-[var(--adm-surface-3)]"
              >
                <span className="flex h-8 w-8 items-center justify-center rounded-md bg-[var(--adm-accent-soft)] text-[var(--adm-accent-text)]">
                  <AdminIcon name={action.icon} size={16} />
                </span>
                <span className="adm-text text-[13px] font-semibold">
                  {action.label}
                </span>
              </Link>
            ) : (
              <span
                aria-disabled
                className="flex cursor-not-allowed items-center justify-between gap-2.5 rounded-lg border border-[var(--adm-border)] bg-[var(--adm-surface-2)] px-3 py-2.5 opacity-60"
              >
                <span className="flex items-center gap-2.5">
                  <span className="flex h-8 w-8 items-center justify-center rounded-md bg-[var(--adm-surface-3)] text-[var(--adm-text-muted)]">
                    <AdminIcon name={action.icon} size={16} />
                  </span>
                  <span className="adm-text text-[13px] font-semibold">
                    {action.label}
                  </span>
                </span>
                <span className="rounded bg-[var(--adm-surface-3)] px-1.5 py-0.5 text-[10px] font-bold text-[var(--adm-text-muted)]">
                  به‌زودی
                </span>
              </span>
            )}
          </li>
        ))}
      </ul>
    </WidgetCard>
  );
}
