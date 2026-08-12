import Link from "next/link";
import type { AdminIconName } from "@/lib/admin/navigation";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { WidgetCard } from "@/components/admin/dashboard/widgets/widget-card";

type ModuleEntry = {
  label: string;
  href: string;
  icon: AdminIconName;
  ready: boolean;
};

// Reflects the real implementation status of Admin modules (navigation truth),
// not fabricated metrics.
const MODULES: ModuleEntry[] = [
  { label: "کاربران", href: ADMIN_ROUTES.users, icon: "users", ready: true },
  { label: "محتوا", href: ADMIN_ROUTES.content, icon: "content", ready: false },
  { label: "آموزش", href: ADMIN_ROUTES.learning, icon: "learning", ready: false },
  { label: "ابزارها", href: ADMIN_ROUTES.toolbox, icon: "toolbox", ready: false },
  { label: "پرامپت‌ها", href: ADMIN_ROUTES.promptLab, icon: "prompt", ready: false },
  { label: "تحلیل‌ها", href: ADMIN_ROUTES.analytics, icon: "analytics", ready: false },
  { label: "Audit", href: ADMIN_ROUTES.audit, icon: "audit", ready: false },
  { label: "عملیات", href: ADMIN_ROUTES.operations, icon: "outbox", ready: false },
];

/** Overview of Admin module readiness with quick navigation. */
export function ModuleStatusCard() {
  return (
    <WidgetCard title="وضعیت ماژول‌ها" icon="dashboard" className="h-full">
      <ul className="grid grid-cols-2 gap-2">
        {MODULES.map((module) => (
          <li key={module.label}>
            <Link
              href={module.href}
              className="adm-focus flex items-center justify-between gap-2 rounded-lg bg-[var(--adm-surface-2)] px-3 py-2 hover:bg-[var(--adm-surface-3)]"
            >
              <span className="flex min-w-0 items-center gap-2">
                <AdminIcon
                  name={module.icon}
                  size={15}
                  className="text-[var(--adm-text-muted)]"
                />
                <span className="adm-text truncate text-[12px] font-medium">
                  {module.label}
                </span>
              </span>
              <span
                className={`shrink-0 rounded px-1.5 py-0.5 text-[10px] font-bold ${
                  module.ready
                    ? "bg-[var(--adm-success-soft)] text-[var(--adm-success)]"
                    : "bg-[var(--adm-surface-3)] text-[var(--adm-text-muted)]"
                }`}
              >
                {module.ready ? "فعال" : "در حال توسعه"}
              </span>
            </Link>
          </li>
        ))}
      </ul>
    </WidgetCard>
  );
}
