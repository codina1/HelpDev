"use client";

import Link from "next/link";
import { useAuth } from "@/components/auth";
import { USER_PANEL_ROUTE } from "@/lib/admin/routes";
import { AdminMenu } from "@/components/admin/shared/admin-menu";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { getUserDisplayName, getUserInitials } from "@/types/auth";

const ROLE_LABEL: Record<string, string> = {
  Admin: "مدیر",
  Writer: "نویسنده",
  User: "کاربر",
};

export function AdminUserMenu() {
  const { user, logout } = useAuth();
  if (!user) return null;

  const displayName = getUserDisplayName(user);
  const initials = getUserInitials(user);
  const roleLabel = ROLE_LABEL[user.role] ?? user.role;

  return (
    <AdminMenu
      label="حساب کاربری"
      panelClassName="w-[240px]"
      trigger={({ toggle, triggerProps }) => (
        <button
          type="button"
          onClick={toggle}
          className="adm-focus flex items-center gap-2 rounded-lg p-1 pe-2 adm-hover"
          aria-label="منوی حساب کاربری"
          {...triggerProps}
        >
          <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-[var(--adm-accent-soft)] text-[12px] font-bold text-[var(--adm-accent-text)]">
            {initials}
          </span>
          <span className="hidden min-w-0 flex-col items-start leading-tight md:flex">
            <span className="adm-text max-w-[120px] truncate text-[12px] font-bold">
              {displayName}
            </span>
            <span className="adm-subtle text-[10px]">{roleLabel}</span>
          </span>
        </button>
      )}
    >
      {({ close }) => (
        <div role="none">
          <div className="adm-border-b px-3 pb-3 pt-1">
            <p className="adm-text truncate text-[13px] font-bold">{displayName}</p>
            <p className="adm-subtle mt-0.5 text-[11px]">{roleLabel}</p>
          </div>
          <div className="py-1">
            <MenuLink href={USER_PANEL_ROUTE} icon="users" label="رفتن به پنل کاربری" onClick={close} />
            <MenuLink href={`${USER_PANEL_ROUTE}?tab=settings`} icon="settings" label="تنظیمات حساب" onClick={close} />
          </div>
          <div className="adm-border-b" />
          <div className="py-1">
            <button
              type="button"
              role="menuitem"
              onClick={() => {
                close();
                logout();
              }}
              className="adm-focus flex w-full items-center gap-2 rounded-lg px-3 py-2 text-[13px] font-semibold text-[var(--adm-danger)] adm-hover"
            >
              <AdminIcon name="logout" size={16} />
              خروج از حساب
            </button>
          </div>
        </div>
      )}
    </AdminMenu>
  );
}

function MenuLink({
  href,
  icon,
  label,
  onClick,
}: {
  href: string;
  icon: "users" | "settings";
  label: string;
  onClick: () => void;
}) {
  return (
    <Link
      href={href}
      role="menuitem"
      onClick={onClick}
      className="adm-focus flex items-center gap-2 rounded-lg px-3 py-2 text-[13px] adm-muted adm-hover"
    >
      <AdminIcon name={icon} size={16} />
      {label}
    </Link>
  );
}
