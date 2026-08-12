"use client";

import Link from "next/link";
import { useCallback, useState } from "react";
import { useRouter } from "next/navigation";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { useAdminContentDetail } from "@/lib/admin/content/content-hooks";
import type { ContentStatusValue } from "@/lib/admin/content/content-types";
import { normalizeContentStatus } from "@/lib/admin/content/content-mappers";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { AdminEmptyState } from "@/components/admin/feedback/admin-empty-state";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { ContentDetailTabs } from "@/components/admin/content/details/content-detail-tabs";
import { WorkflowPanel } from "@/components/admin/content/workflow/workflow-panel";

export function ContentWorkflowWorkspace({ contentId }: { contentId: string }) {
  const router = useRouter();
  const { data: content, loading, error, reload } = useAdminContentDetail(contentId);
  const [status, setStatus] = useState<ContentStatusValue | null>(null);

  const effectiveStatus = status ?? (content ? content.status : "Draft");

  const handleStatusChange = useCallback((next: ContentStatusValue) => {
    setStatus(next);
    router.refresh();
  }, [router]);

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="گردش کار محتوا"
        description={content?.title ?? "مراحل تأیید و انتشار این محتوا"}
        secondaryActions={
          <Link
            href={ADMIN_ROUTES.content}
            className="adm-btn adm-btn-outline adm-focus inline-flex items-center gap-1.5"
          >
            <AdminIcon name="chevron" size={16} />
            بازگشت
          </Link>
        }
        primaryAction={
          content ? (
            <Link
              href={`${ADMIN_ROUTES.content}/${encodeURIComponent(content.id)}/edit`}
              className="adm-btn adm-btn-primary adm-focus inline-flex items-center gap-1.5"
            >
              <AdminIcon name="content" size={16} />
              ویرایش
            </Link>
          ) : undefined
        }
      />

      {content ? <ContentDetailTabs id={content.id} active="workflow" /> : null}

      {loading ? (
        <AdminLoadingState cards={0} rows={4} />
      ) : error ? (
        <AdminErrorState error={error} onRetry={reload} />
      ) : !content ? (
        <AdminEmptyState
          icon="content"
          title="محتوا یافت نشد"
          description="شناسه وارد شده معتبر نیست یا به آن دسترسی ندارید."
        />
      ) : (
        <WorkflowPanel
          contentId={content.id}
          authorId={content.authorId}
          status={normalizeContentStatus(effectiveStatus)}
          onStatusChange={handleStatusChange}
          showTimeline
        />
      )}
    </div>
  );
}
