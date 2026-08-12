"use client";

import Link from "next/link";
import { useAuth } from "@/components/auth";
import { USER_PANEL_ROUTE } from "@/lib/admin/routes";
import { AdminIcon } from "@/components/admin/shared/admin-icons";

/**
 * Forbidden page for authenticated non-admin users. Explains that Admin access
 * is required without leaking authorization internals or enumerating roles
 * beyond the user's own already-known role.
 */
export function AdminAccessDenied() {
  const { user, logout } = useAuth();

  return (
    <div className="adm-app flex min-h-dvh flex-col items-center justify-center gap-5 px-6 text-center">
      <span className="flex h-16 w-16 items-center justify-center rounded-2xl bg-[var(--adm-danger-soft)] text-[var(--adm-danger)]">
        <AdminIcon name="shield" size={32} />
      </span>
      <div className="space-y-2">
        <h1 className="adm-text text-2xl font-bold">دسترسی غیرمجاز</h1>
        <p className="adm-muted max-w-md text-[13px] leading-7">
          این بخش مخصوص مدیران است و برای مشاهده آن به دسترسی مدیریت نیاز دارید.
          {user ? " با حساب فعلی شما امکان ورود به این بخش وجود ندارد." : null}
        </p>
      </div>
      <div className="flex flex-wrap items-center justify-center gap-2">
        <Link href={USER_PANEL_ROUTE} className="adm-btn adm-btn-primary adm-focus">
          رفتن به پنل کاربری
        </Link>
        <button type="button" onClick={logout} className="adm-btn adm-btn-outline adm-focus">
          خروج از حساب
        </button>
      </div>
    </div>
  );
}
