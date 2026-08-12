"use client";

import Link from "next/link";
import { useAuth } from "@/components/auth";
import { ADMIN_QUICK_CREATE } from "@/lib/admin/command-menu";
import { getPermissionsForRole, hasPermission } from "@/lib/admin/permissions";
import { AdminMenu } from "@/components/admin/shared/admin-menu";
import { AdminIcon } from "@/components/admin/shared/admin-icons";

export function AdminQuickCreate() {
  const { user } = useAuth();
  const permissions = getPermissionsForRole(user?.role);

  const items = ADMIN_QUICK_CREATE.filter((item) =>
    hasPermission(permissions, item.permission),
  );

  if (items.length === 0) return null;

  return (
    <AdminMenu
      label="ایجاد سریع"
      panelClassName="w-[220px]"
      trigger={({ toggle, triggerProps }) => (
        <button
          type="button"
          onClick={toggle}
          className="adm-btn adm-btn-primary adm-focus"
          {...triggerProps}
        >
          <AdminIcon name="plus" size={16} />
          <span className="hidden sm:inline">ایجاد</span>
        </button>
      )}
    >
      {({ close }) => (
        <div role="none">
          <p className="adm-subtle px-3 pb-1 pt-1 text-[10px] font-bold uppercase tracking-wide">
            ایجاد سریع
          </p>
          {items.map((item) =>
            item.status === "ready" && item.href ? (
              <Link
                key={item.id}
                href={item.href}
                role="menuitem"
                onClick={close}
                className="adm-focus flex items-center gap-2 rounded-lg px-3 py-2 text-[13px] adm-muted adm-hover"
              >
                <AdminIcon name={item.icon} size={16} />
                {item.title}
              </Link>
            ) : (
              <span
                key={item.id}
                aria-disabled="true"
                title="به‌زودی"
                className="flex cursor-not-allowed items-center gap-2 rounded-lg px-3 py-2 text-[13px] adm-subtle opacity-60"
              >
                <AdminIcon name={item.icon} size={16} />
                {item.title}
                <span className="ms-auto rounded bg-[var(--adm-surface-3)] px-1.5 py-0.5 text-[9px] font-bold">
                  به‌زودی
                </span>
              </span>
            ),
          )}
        </div>
      )}
    </AdminMenu>
  );
}
