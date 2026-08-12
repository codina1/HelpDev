import type { Metadata } from "next";
import { AdminIcon } from "@/components/admin/shared/admin-icons";

export const metadata: Metadata = { title: "تنظیمات" };

/**
 * System settings foundation. AI toggles are documented keys only —
 * secrets (API keys) are never shown here.
 */
export default function AdminSettingsPage() {
  return (
    <div className="space-y-6">
      <header className="space-y-2">
        <h1 className="adm-text inline-flex items-center gap-2 text-[20px] font-bold">
          <AdminIcon name="settings" size={22} />
          تنظیمات سامانه
        </h1>
        <p className="adm-subtle max-w-2xl text-[13px] leading-6">
          پیکربندی عمومی، Feature Flags و تنظیمات مدیریتی. مقادیر حساس (کلید API) از طریق
          پیکربندی سرور مدیریت می‌شوند و در این صفحه نمایش داده نمی‌شوند.
        </p>
      </header>

      <section
        aria-labelledby="ai-settings-heading"
        className="space-y-3 rounded-xl border border-[var(--adm-border)] bg-[var(--adm-surface)] p-5"
      >
        <h2 id="ai-settings-heading" className="adm-text text-[15px] font-bold">
          دستیار هوش مصنوعی (پایه)
        </h2>
        <p className="adm-subtle text-[12px] leading-6">
          کلیدهای پیشنهادی Administration (بدون راز):{" "}
          <span dir="ltr" className="font-mono text-[11px]">
            Ai.Enabled
          </span>
          ،{" "}
          <span dir="ltr" className="font-mono text-[11px]">
            Ai.DefaultModel
          </span>
          ،{" "}
          <span dir="ltr" className="font-mono text-[11px]">
            Ai.AllowedTasks
          </span>
          . در v1 کنترل اجرایی از بخش{" "}
          <span dir="ltr" className="font-mono text-[11px]">
            Ai
          </span>{" "}
          در پیکربندی سرور خوانده می‌شود.
        </p>
        <ul className="adm-subtle list-disc space-y-1 pe-5 text-[12px] leading-6">
          <li>فعال/غیرفعال بودن دستیار</li>
          <li>مدل پیش‌فرض (نام نمایشی — نه کلید API)</li>
          <li>فهرست وظایف مجاز</li>
        </ul>
      </section>
    </div>
  );
}
