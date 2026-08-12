import Link from "next/link";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { AdminEmptyState } from "@/components/admin/feedback/admin-empty-state";

type ContentEmptyStateProps = {
  filtered: boolean;
  /** When true and not filtered, show the Writer-scoped empty message. */
  writerScoped?: boolean;
  onClearFilters?: () => void;
};

/** Distinguishes global empty, writer-scoped empty, and filtered-empty. */
export function ContentEmptyState({
  filtered,
  writerScoped = false,
  onClearFilters,
}: ContentEmptyStateProps) {
  if (filtered) {
    return (
      <AdminEmptyState
        icon="content"
        title="محتوایی با فیلترهای انتخاب‌شده پیدا نشد"
        description="عبارت جستجو یا فیلترها را تغییر دهید."
        primaryAction={
          onClearFilters ? (
            <button type="button" onClick={onClearFilters} className="adm-btn adm-btn-outline adm-focus">
              پاک کردن فیلترها
            </button>
          ) : undefined
        }
      />
    );
  }

  if (writerScoped) {
    return (
      <AdminEmptyState
        icon="content"
        title="هنوز محتوایی برای این حساب ثبت نشده است"
        description="اولین محتوای خود را بسازید."
        primaryAction={
          <Link href={ADMIN_ROUTES.contentNew} className="adm-btn adm-btn-primary adm-focus">
            محتوای جدید
          </Link>
        }
      />
    );
  }

  return (
    <AdminEmptyState
      icon="content"
      title="هنوز محتوایی ایجاد نشده است"
      description="اولین محتوای HelpDev را بسازید."
      primaryAction={
        <Link href={ADMIN_ROUTES.contentNew} className="adm-btn adm-btn-primary adm-focus">
          محتوای جدید
        </Link>
      }
    />
  );
}
