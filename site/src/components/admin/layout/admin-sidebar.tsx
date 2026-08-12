"use client";

import { useMemo } from "react";
import { usePathname } from "next/navigation";
import { useAuth } from "@/components/auth";
import { ADMIN_NAVIGATION, filterAdminNavigation } from "@/lib/admin/navigation";
import { findActiveNavItemId } from "@/lib/admin/route-matcher";
import { useAdminPreferences } from "@/components/admin/admin-preferences-provider";
import { AdminNavGroup } from "@/components/admin/navigation/admin-nav-group";
import { AdminLogo } from "@/components/admin/shared/admin-logo";
import { AdminIcon } from "@/components/admin/shared/admin-icons";

/** Sticky, collapsible desktop sidebar (hidden below the `lg` breakpoint). */
export function AdminSidebar() {
  const pathname = usePathname();
  const { user } = useAuth();
  const { preferences, toggleSidebar } = useAdminPreferences();

  const navigation = useMemo(
    () => filterAdminNavigation(ADMIN_NAVIGATION, user?.role),
    [user?.role],
  );
  const activeItemId = useMemo(
    () => findActiveNavItemId(navigation, pathname),
    [navigation, pathname],
  );

  return (
    <aside className="adm-sidebar-desktop adm-surface sticky top-0 h-dvh flex-col border-0 border-e border-[var(--adm-border)]">
      <div className="flex h-16 items-center justify-between px-3">
        <AdminLogo />
        <button
          type="button"
          onClick={toggleSidebar}
          className="adm-icon-btn adm-focus"
          aria-label={preferences.sidebarCollapsed ? "باز کردن نوار کناری" : "جمع کردن نوار کناری"}
          aria-pressed={preferences.sidebarCollapsed}
        >
          <AdminIcon name={preferences.sidebarCollapsed ? "expand" : "collapse"} size={18} />
        </button>
      </div>

      <nav
        aria-label="ناوبری اصلی مدیریت"
        className="adm-scroll flex-1 space-y-3 overflow-y-auto px-2 py-3"
      >
        {navigation.map((group) => (
          <AdminNavGroup
            key={group.id}
            group={group}
            activeItemId={activeItemId}
            variant="desktop"
          />
        ))}
      </nav>
    </aside>
  );
}
