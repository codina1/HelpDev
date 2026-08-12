"use client";

import { useEffect, useMemo, useRef } from "react";
import { usePathname } from "next/navigation";
import { useAuth } from "@/components/auth";
import { ADMIN_NAVIGATION, filterAdminNavigation } from "@/lib/admin/navigation";
import { findActiveNavItemId } from "@/lib/admin/route-matcher";
import { AdminNavGroup } from "@/components/admin/navigation/admin-nav-group";
import { AdminLogo } from "@/components/admin/shared/admin-logo";
import { AdminIcon } from "@/components/admin/shared/admin-icons";

type AdminMobileDrawerProps = {
  open: boolean;
  onClose: () => void;
};

/** Off-canvas navigation drawer for mobile/tablet. Opens from the inline-end. */
export function AdminMobileDrawer({ open, onClose }: AdminMobileDrawerProps) {
  const pathname = usePathname();
  const { user } = useAuth();
  const panelRef = useRef<HTMLDivElement>(null);

  const navigation = useMemo(
    () => filterAdminNavigation(ADMIN_NAVIGATION, user?.role),
    [user?.role],
  );
  const activeItemId = useMemo(
    () => findActiveNavItemId(navigation, pathname),
    [navigation, pathname],
  );

  useEffect(() => {
    if (!open) return;
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") onClose();
    };
    document.addEventListener("keydown", onKeyDown);
    panelRef.current?.focus();
    return () => document.removeEventListener("keydown", onKeyDown);
  }, [open, onClose]);

  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 lg:hidden" role="dialog" aria-modal="true" aria-label="ناوبری مدیریت">
      <button
        type="button"
        aria-label="بستن منو"
        onClick={onClose}
        className="absolute inset-0 bg-black/50"
      />
      <div
        ref={panelRef}
        tabIndex={-1}
        className="adm-surface adm-animate-in absolute inset-y-0 end-0 flex w-[280px] max-w-[85vw] flex-col outline-none"
      >
        <div className="adm-border-b flex h-16 items-center justify-between px-3">
          <AdminLogo />
          <button
            type="button"
            onClick={onClose}
            className="adm-icon-btn adm-focus"
            aria-label="بستن منو"
          >
            <AdminIcon name="close" size={18} />
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
              variant="mobile"
              onNavigate={onClose}
            />
          ))}
        </nav>
      </div>
    </div>
  );
}
