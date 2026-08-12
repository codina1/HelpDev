"use client";

import { useMemo } from "react";
import { usePathname } from "next/navigation";
import { buildAdminBreadcrumbs } from "@/lib/admin/breadcrumbs";
import { AdminBreadcrumb } from "@/components/admin/navigation/admin-breadcrumb";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { EnvironmentBadge } from "@/components/admin/shared/environment-badge";
import { AdminThemeSwitcher } from "@/components/admin/shared/admin-theme-switcher";
import { AdminNotificationsButton } from "@/components/admin/shared/admin-notifications-button";
import { AdminUserMenu } from "@/components/admin/shared/admin-user-menu";
import { AdminQuickCreate } from "@/components/admin/command/admin-quick-create";
import { useAdminCommand } from "@/components/admin/command/admin-command-palette";

type AdminHeaderProps = {
  onOpenMobileNav: () => void;
};

export function AdminHeader({ onOpenMobileNav }: AdminHeaderProps) {
  const pathname = usePathname();
  const { open: openCommand } = useAdminCommand();
  const breadcrumbs = useMemo(() => buildAdminBreadcrumbs(pathname), [pathname]);

  return (
    <header className="adm-surface adm-border-b sticky top-0 z-30 flex h-16 items-center gap-2 border-0 border-b px-3 sm:px-4">
      <button
        type="button"
        onClick={onOpenMobileNav}
        className="adm-icon-btn adm-focus lg:hidden"
        aria-label="باز کردن منو"
      >
        <AdminIcon name="menu" size={20} />
      </button>

      <div className="hidden min-w-0 flex-1 md:block">
        <AdminBreadcrumb items={breadcrumbs} />
      </div>

      <button
        type="button"
        onClick={openCommand}
        className="adm-focus mx-auto flex h-9 max-w-sm flex-1 items-center gap-2 rounded-lg border border-[var(--adm-border-strong)] bg-[var(--adm-bg-subtle)] px-3 text-[13px] adm-subtle hover:border-[var(--adm-accent)] md:mx-0"
        aria-label="جستجو و فرمان‌ها"
      >
        <AdminIcon name="search" size={16} />
        <span className="flex-1 text-start">جستجو...</span>
        <kbd className="hidden rounded border border-[var(--adm-border)] px-1.5 py-0.5 text-[10px] sm:inline">
          Ctrl K
        </kbd>
      </button>

      <div className="flex shrink-0 items-center gap-1 sm:gap-2">
        <div className="hidden sm:block">
          <AdminQuickCreate />
        </div>
        <AdminNotificationsButton />
        <div className="hidden sm:block">
          <EnvironmentBadge />
        </div>
        <AdminThemeSwitcher />
        <AdminUserMenu />
      </div>
    </header>
  );
}
