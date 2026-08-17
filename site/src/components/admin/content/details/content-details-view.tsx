"use client";

import Link from "next/link";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { useAdminContentDetail } from "@/lib/admin/content/content-hooks";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { AdminEmptyState } from "@/components/admin/feedback/admin-empty-state";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { ContentDetailsCard } from "@/components/admin/content/details/content-details-card";
import { ContentMetaCard } from "@/components/admin/content/details/content-meta-card";
import { ContentDetailTabs } from "@/components/admin/content/details/content-detail-tabs";
import { RelatedKnowledgePanel } from "@/components/admin/content/related/related-knowledge-panel";
import { resolveContentCoverUrl } from "@/lib/admin/content/content-mappers";
import type { AdminContentDetail, ContentDetail } from "@/lib/admin/content/content-types";

/** Adapts the Admin Read Model to the existing overview cards' view model. */
function toContentDetail(admin: AdminContentDetail): ContentDetail {
  return {
    id: admin.id,
    title: admin.title,
    slug: admin.slug,
    body: admin.body,
    coverImage: resolveContentCoverUrl(admin.coverImage, admin.seo.ogImage),
    type: admin.type,
    typeLabel: admin.typeLabel,
    authorId: admin.authorId,
    status: admin.status,
    statusLabel: admin.statusLabel,
    views: admin.views,
    saves: admin.saves,
    createdAt: admin.createdAtUtc,
  };
}

/**
 * `/admin/content/[id]` — details/preview. The route segment carries the
 * content id (Guid); data is loaded from the Admin Read Model, so drafts and
 * SEO/timestamps are available without depending on the public API.
 */
export function ContentDetailsView({ id }: { id: string }) {
  const { data, loading, error, reload } = useAdminContentDetail(id);

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="جزئیات محتوا"
        description="پیش‌نمایش و مدیریت این محتوا"
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
          data ? (
            <Link
              href={`${ADMIN_ROUTES.content}/${encodeURIComponent(data.id)}/edit`}
              className="adm-btn adm-btn-primary adm-focus inline-flex items-center gap-1.5"
            >
              <AdminIcon name="content" size={16} />
              ویرایش
            </Link>
          ) : undefined
        }
      />

      {data ? <ContentDetailTabs id={data.id} active="overview" /> : null}

      {loading ? (
        <AdminLoadingState cards={0} rows={6} />
      ) : error ? (
        <AdminErrorState error={error} onRetry={reload} />
      ) : !data ? (
        <AdminEmptyState
          icon="content"
          title="محتوا یافت نشد"
          description="این محتوا وجود ندارد یا به آن دسترسی ندارید."
        />
      ) : (
        <div className="space-y-4">
          <div className="grid grid-cols-1 gap-4 lg:grid-cols-[2fr_1fr]">
            <ContentDetailsCard content={toContentDetail(data)} />
            <ContentMetaCard content={toContentDetail(data)} />
          </div>
          <RelatedKnowledgePanel contentId={data.id} />
        </div>
      )}
    </div>
  );
}
