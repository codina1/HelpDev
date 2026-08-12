"use client";

import { useState, type ReactNode } from "react";
import { AdminPreferencesProvider } from "@/components/admin/admin-preferences-provider";
import { AdminGuard } from "@/components/admin/admin-guard";
import { AdminCommandProvider } from "@/components/admin/command/admin-command-palette";
import { AdminSidebar } from "@/components/admin/layout/admin-sidebar";
import { AdminHeader } from "@/components/admin/layout/admin-header";
import { AdminContentArea } from "@/components/admin/layout/admin-content-area";
import { AdminFooter } from "@/components/admin/layout/admin-footer";
import { AdminMobileDrawer } from "@/components/admin/layout/admin-mobile-drawer";

/**
 * Root Admin application shell. Fully independent of the user-panel `AppShell`:
 * its own sidebar, header, content area, theming, guard and command palette.
 */
export function AdminShell({ children }: { children: ReactNode }) {
  return (
    <AdminPreferencesProvider>
      <AdminGuard>
        <AdminLayout>{children}</AdminLayout>
      </AdminGuard>
    </AdminPreferencesProvider>
  );
}

function AdminLayout({ children }: { children: ReactNode }) {
  const [mobileNavOpen, setMobileNavOpen] = useState(false);

  return (
    <AdminCommandProvider>
      <div className="adm-app">
        <a
          href="#admin-main"
          className="adm-focus sr-only focus:not-sr-only focus:absolute focus:z-[70] focus:m-2 focus:rounded-lg focus:bg-[var(--adm-accent)] focus:px-3 focus:py-2 focus:text-[13px] focus:text-[var(--adm-accent-fg)]"
        >
          پرش به محتوا
        </a>

        <div className="adm-shell-grid">
          <AdminSidebar />
          <div className="flex min-h-dvh min-w-0 flex-col">
            <AdminHeader onOpenMobileNav={() => setMobileNavOpen(true)} />
            <AdminContentArea>{children}</AdminContentArea>
            <AdminFooter />
          </div>
        </div>

        <AdminMobileDrawer
          open={mobileNavOpen}
          onClose={() => setMobileNavOpen(false)}
        />
      </div>
    </AdminCommandProvider>
  );
}
