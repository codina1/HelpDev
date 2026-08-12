import Link from "next/link";
import { AdminEmptyState } from "@/components/admin/feedback/admin-empty-state";
import type { ContentWorkspaceDefinition } from "@/lib/admin/content/factory";

type WorkspaceEmptyStateProps = {
  workspace: ContentWorkspaceDefinition;
  filtered?: boolean;
  onClearFilters?: () => void;
};

export function WorkspaceEmptyState({
  workspace,
  filtered = false,
  onClearFilters,
}: WorkspaceEmptyStateProps) {
  if (filtered) {
    return (
      <AdminEmptyState
        icon={workspace.icon}
        title="نتیجه‌ای یافت نشد"
        description="با فیلترهای فعلی موردی در این فضای کار نیست."
        primaryAction={
          onClearFilters ? (
            <button type="button" className="adm-btn adm-btn-outline adm-focus" onClick={onClearFilters}>
              پاک کردن فیلترها
            </button>
          ) : undefined
        }
      />
    );
  }

  return (
    <AdminEmptyState
      icon={workspace.icon}
      title={`هنوز موردی در «${workspace.title}» نیست`}
      description="اولین مورد را بسازید یا از فهرست همه محتواها وارد شوید."
      primaryAction={
        <Link href={workspace.createHref} className="adm-btn adm-btn-primary adm-focus">
          {workspace.createTitle}
        </Link>
      }
    />
  );
}
