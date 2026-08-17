import Link from "next/link";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { AdminSurface } from "@/components/admin/page/admin-surface";
import { AdminIcon } from "@/components/admin/shared/admin-icons";

type WriterPromptEmptyStateProps = {
  filtered: boolean;
  onClearFilters?: () => void;
};

/** Empty state when the writer has no prompts or filters hide all rows. */
export function WriterPromptEmptyState({ filtered, onClearFilters }: WriterPromptEmptyStateProps) {
  return (
    <AdminSurface className="flex flex-col items-center gap-4 py-10 text-center">
      <AdminIcon name="prompt" size={32} />
      <div className="space-y-2">
        <h2 className="adm-text text-[15px] font-bold">
          {filtered ? "پرامپتی با این فیلتر نیست" : "هنوز پرامپتی نساخته‌اید"}
        </h2>
        <p className="adm-muted mx-auto max-w-md text-[13px] leading-6">
          {filtered
            ? "فیلتر وضعیت را تغییر دهید یا همه پرامپت‌ها را ببینید."
            : "اولین پرامپت خود را بسازید و برای بررسی ارسال کنید."}
        </p>
      </div>
      <div className="flex flex-wrap justify-center gap-2">
        {!filtered ? (
          <Link href={ADMIN_ROUTES.contentPromptsNew} className="adm-btn adm-btn-primary adm-focus">
            ایجاد پرامپت
          </Link>
        ) : null}
        {filtered && onClearFilters ? (
          <button type="button" className="adm-btn adm-btn-outline adm-focus" onClick={onClearFilters}>
            پاک کردن فیلتر
          </button>
        ) : null}
      </div>
    </AdminSurface>
  );
}
