import Link from "next/link";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { EnvironmentBadge } from "@/components/admin/shared/environment-badge";

/** Section top — page header for the Admin Command Center. */
export function DashboardHeader() {
  return (
    <AdminPageHeader
      title="داشبورد مدیریت"
      description="مرکز کنترل HelpDev"
      badge={<EnvironmentBadge />}
      primaryAction={
        <Link
          href={ADMIN_ROUTES.contentNew}
          className="adm-btn adm-btn-primary adm-focus inline-flex items-center gap-1.5"
        >
          <AdminIcon name="plus" size={16} />
          ایجاد محتوا
        </Link>
      }
      secondaryActions={
        <Link
          href={ADMIN_ROUTES.operations}
          className="adm-btn adm-btn-outline adm-focus inline-flex items-center gap-1.5"
        >
          <AdminIcon name="settings" size={16} />
          مدیریت سیستم
        </Link>
      }
    />
  );
}
