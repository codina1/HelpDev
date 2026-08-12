"use client";

import { useState, type ReactNode } from "react";
import { DashboardSidebar } from "@/components/account/dashboard/dashboard-sidebar";
import { DashboardTopbar } from "@/components/account/dashboard/dashboard-topbar";
import type { AccountSection } from "@/lib/account-menu";
import type { AuthUser } from "@/types/auth";

type AccountDashboardShellProps = {
  user: AuthUser;
  activeSection: AccountSection;
  onSectionChange: (section: AccountSection) => void;
  children: ReactNode;
};

export function AccountDashboardShell({
  user,
  activeSection,
  onSectionChange,
  children,
}: AccountDashboardShellProps) {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  function handleSectionChange(section: AccountSection) {
    onSectionChange(section);
    setMobileMenuOpen(false);
  }

  return (
    <div className="account-dashboard min-h-dvh bg-[#080a12] lg:grid lg:grid-cols-[260px_minmax(0,1fr)]">
      <DashboardSidebar
        user={user}
        activeSection={activeSection}
        onSectionChange={handleSectionChange}
        mobileOpen={mobileMenuOpen}
        onMobileClose={() => setMobileMenuOpen(false)}
      />

      <div className="account-dashboard-main flex min-h-dvh min-w-0 flex-col">
        <DashboardTopbar user={user} onMenuClick={() => setMobileMenuOpen(true)} />
        <main className="flex-1 overflow-y-auto px-4 py-5 lg:px-6 lg:py-6">
          <div className="mx-auto max-w-[1200px] space-y-5">{children}</div>
        </main>
      </div>
    </div>
  );
}
