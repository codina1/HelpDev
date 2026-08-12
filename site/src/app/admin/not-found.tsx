import Link from "next/link";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { AdminIcon } from "@/components/admin/shared/admin-icons";

export default function AdminNotFound() {
  return (
    <div className="flex min-h-[60vh] flex-col items-center justify-center gap-5 text-center">
      <span className="flex h-16 w-16 items-center justify-center rounded-2xl bg-[var(--adm-surface-3)] text-[var(--adm-text-muted)]">
        <AdminIcon name="search" size={30} />
      </span>
      <div className="space-y-2">
        <h1 className="adm-text text-2xl font-bold">صفحه پیدا نشد</h1>
        <p className="adm-muted max-w-md text-[13px] leading-7">
          صفحه‌ای که دنبال آن بودید در پنل مدیریت وجود ندارد یا جابه‌جا شده است.
        </p>
      </div>
      <Link href={ADMIN_ROUTES.dashboard} className="adm-btn adm-btn-primary adm-focus">
        بازگشت به داشبورد
      </Link>
    </div>
  );
}
