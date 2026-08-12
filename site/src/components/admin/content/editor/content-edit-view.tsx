"use client";

import { useAdminContentDetail } from "@/lib/admin/content/content-hooks";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { AdminEmptyState } from "@/components/admin/feedback/admin-empty-state";
import { ContentStudio } from "@/components/admin/content/editor/content-studio";

/**
 * Loads content by id from the Admin Read Model then renders the Content Studio.
 * Works for drafts and published items and pre-fills excerpt/cover/SEO.
 */
export function ContentEditView({ id }: { id: string }) {
  const { data, loading, error, reload } = useAdminContentDetail(id);

  if (loading) {
    return <AdminLoadingState cards={0} rows={8} />;
  }
  if (error) {
    return <AdminErrorState error={error} onRetry={reload} />;
  }
  if (!data) {
    return (
      <AdminEmptyState
        icon="content"
        title="محتوا یافت نشد"
        description="این محتوا وجود ندارد یا به آن دسترسی ندارید."
      />
    );
  }

  return <ContentStudio initial={data} />;
}
