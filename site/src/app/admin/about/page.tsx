import type { Metadata } from "next";
import Link from "next/link";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { AdminSurface } from "@/components/admin/page/admin-surface";
import { PUBLIC_ABOUT_PATH } from "@/lib/public/about-routes";

export const metadata: Metadata = { title: "درباره ما" };

export default function AdminAboutPage() {
  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="درباره ما"
        description="مدیریت صفحه عمومی درباره ما. این مسیر فقط برای ادمین است و جایگزین تنظیمات سامانه نیست."
      />
      <AdminSurface padding="md" className="space-y-3">
        <p className="adm-muted text-[13px] leading-6">
          نسخهٔ عمومی این صفحه در مسیر جداگانه سرو می‌شود و کاربران عادی به تنظیمات
          هدایت نمی‌شوند.
        </p>
        <p className="adm-subtle text-[12px]" dir="ltr">
          {PUBLIC_ABOUT_PATH}
        </p>
        <Link href={PUBLIC_ABOUT_PATH} className="adm-btn adm-btn-outline adm-focus text-[12px]">
          مشاهده صفحه عمومی
        </Link>
      </AdminSurface>
    </div>
  );
}
