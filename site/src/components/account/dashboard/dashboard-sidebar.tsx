"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useAuth } from "@/components/auth";
import { HelpDevLogo, NavIcon } from "@/components/account/dashboard/dashboard-icons";
import {
  getVisibleAccountMenu,
  type AccountSection,
} from "@/lib/account-menu";
import { SITE } from "@/lib/constants";
import {
  DASHBOARD_MAIN_NAV,
  DASHBOARD_SECONDARY_NAV,
} from "@/data/account-dashboard";
import type { AuthUser } from "@/types/auth";

type DashboardSidebarProps = {
  user: AuthUser;
  activeSection: AccountSection;
  onSectionChange: (section: AccountSection) => void;
  mobileOpen?: boolean;
  onMobileClose?: () => void;
};

export function DashboardSidebar({
  user,
  activeSection,
  onSectionChange,
  mobileOpen = false,
  onMobileClose,
}: DashboardSidebarProps) {
  const pathname = usePathname();
  const { logout } = useAuth();
  const accountMenu = getVisibleAccountMenu(user.role);

  return (
    <>
      {mobileOpen && (
        <button
          type="button"
          className="fixed inset-0 z-40 bg-black/60 backdrop-blur-sm lg:hidden"
          aria-label="بستن منو"
          onClick={onMobileClose}
        />
      )}

      <aside
        className={[
          "account-sidebar z-50 flex w-[260px] flex-col border-e border-white/[0.06] bg-[#0d1019]/98 backdrop-blur-xl transition-transform duration-300",
          "max-lg:fixed max-lg:inset-y-0 max-lg:start-0",
          mobileOpen ? "max-lg:translate-x-0 max-lg:flex" : "max-lg:translate-x-full max-lg:hidden",
          "lg:sticky lg:top-0 lg:flex lg:h-dvh lg:translate-x-0",
        ].join(" ")}
      >
        <div className="flex items-center gap-3 border-b border-white/[0.06] px-5 py-[18px]">
          <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-violet-600/20">
            <HelpDevLogo size={20} />
          </span>
          <span className="text-[16px] font-extrabold tracking-tight text-white">
            {SITE.name}
          </span>
        </div>

        <div className="scrollbar-thin flex-1 overflow-y-auto px-3 py-4">
          <NavGroup label="منوی اصلی">
            {DASHBOARD_MAIN_NAV.map((item) => (
              <SidebarLink
                key={item.href}
                href={item.href}
                icon={item.icon}
                label={item.label}
                active={pathname === item.href}
              />
            ))}
          </NavGroup>

          <NavGroup label="کشف محتوا" className="mt-5">
            {DASHBOARD_SECONDARY_NAV.map((item) => (
              <SidebarTextLink key={item.label} href={item.href} label={item.label} />
            ))}
          </NavGroup>

          <NavGroup label="حساب کاربری" className="mt-5">
            {accountMenu.map((item) => {
              const isActive = activeSection === item.id;
              return (
                <button
                  key={item.id}
                  type="button"
                  onClick={() => onSectionChange(item.id)}
                  className={[
                    "focus-ring flex w-full items-center gap-2.5 rounded-xl px-3 py-2.5 text-[13px] font-semibold transition-all",
                    isActive
                      ? "bg-violet-600 text-white shadow-[0_4px_16px_rgba(124,58,237,0.35)]"
                      : "text-slate-400 hover:bg-white/[0.04] hover:text-slate-200",
                  ].join(" ")}
                >
                  {item.id === "profile" && (
                    <NavIcon
                      name="user"
                      size={16}
                      className={isActive ? "text-white" : "text-slate-500"}
                    />
                  )}
                  {item.label}
                </button>
              );
            })}
            <button
              type="button"
              onClick={logout}
              className="focus-ring mt-0.5 flex w-full items-center rounded-xl px-3 py-2.5 text-[13px] font-semibold text-red-400/90 transition-colors hover:bg-red-500/10 hover:text-red-300"
            >
              خروج
            </button>
          </NavGroup>
        </div>

        <div className="border-t border-white/[0.06] p-4">
          <div className="rounded-2xl border border-white/[0.08] bg-[#121622] p-4">
            <div className="flex items-center gap-2">
              <span className="text-amber-400">
                <NavIcon name="crown" size={18} className="text-amber-400" />
              </span>
              <span className="text-[13px] font-bold text-white">نسخه Pro</span>
            </div>
            <p className="mt-1.5 text-[11px] leading-5 text-slate-500">
              دسترسی به دوره‌های ویژه و ابزارهای پیشرفته
            </p>
            <button
              type="button"
              className="focus-ring mt-3 w-full rounded-xl bg-violet-600 py-2 text-[12px] font-bold text-white transition-colors hover:bg-violet-500"
            >
              ارتقا به Pro
            </button>
          </div>
        </div>
      </aside>
    </>
  );
}

function NavGroup({
  label,
  children,
  className = "",
}: {
  label: string;
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <div className={className}>
      <p className="mb-1.5 px-3 text-[10px] font-bold text-slate-600">{label}</p>
      <div className="space-y-0.5">{children}</div>
    </div>
  );
}

function SidebarLink({
  href,
  icon,
  label,
  active,
}: {
  href: string;
  icon: string;
  label: string;
  active?: boolean;
}) {
  return (
    <Link
      href={href}
      className={[
        "focus-ring flex items-center gap-2.5 rounded-xl px-3 py-2.5 text-[13px] font-semibold transition-colors",
        active
          ? "bg-white/[0.06] text-white"
          : "text-slate-400 hover:bg-white/[0.04] hover:text-slate-200",
      ].join(" ")}
    >
      <NavIcon
        name={icon}
        size={16}
        className={active ? "text-violet-300" : "text-slate-500"}
      />
      {label}
    </Link>
  );
}

function SidebarTextLink({ href, label }: { href: string; label: string }) {
  return (
    <Link
      href={href}
      className="focus-ring block rounded-xl px-3 py-2 text-[13px] font-medium text-slate-500 transition-colors hover:bg-white/[0.04] hover:text-slate-300"
    >
      {label}
    </Link>
  );
}
