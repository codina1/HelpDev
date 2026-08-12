import type { AdminIconName } from "@/lib/admin/navigation";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { AdminEmptyState } from "@/components/admin/feedback/admin-empty-state";

type AdminModulePlaceholderProps = {
  title: string;
  description: string;
  icon?: AdminIconName;
  emptyTitle?: string;
  emptyDescription?: string;
};

/**
 * In-shell placeholder for modules whose deep functionality lands in a later
 * phase. Clearly communicates a "در حال توسعه" state — it is not a fake
 * functional screen.
 */
export function AdminModulePlaceholder({
  title,
  description,
  icon = "content",
  emptyTitle = "این بخش در حال توسعه است",
  emptyDescription = "قابلیت‌های کامل این بخش در فازهای بعدی پنل مدیریت اضافه می‌شود.",
}: AdminModulePlaceholderProps) {
  return (
    <div className="space-y-5">
      <AdminPageHeader
        title={title}
        description={description}
        badge={
          <span className="rounded-md bg-[var(--adm-warning-soft)] px-2 py-0.5 text-[10px] font-bold text-[var(--adm-warning)]">
            در حال توسعه
          </span>
        }
      />
      <AdminEmptyState icon={icon} title={emptyTitle} description={emptyDescription} />
    </div>
  );
}
