"use client";

import { Suspense, useCallback, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { formatNumberFa } from "@/lib/admin/dashboard/dashboard-mappers";
import { useAdminMediaList } from "@/lib/admin/media/media-hooks";
import {
  buildAdminMediaListHref,
  isAdminMediaListFiltered,
  mergeAdminMediaListQuery,
  parseAdminMediaListQuery,
} from "@/lib/admin/media/media-url-state";
import {
  DEFAULT_ADMIN_MEDIA_LIST_QUERY,
  type AdminMediaListQuery,
} from "@/lib/admin/media/media-types";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { AdminPageSection } from "@/components/admin/page/admin-page-section";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { MediaToolbar } from "@/components/admin/media/media-toolbar";
import { MediaGrid } from "@/components/admin/media/media-grid";
import { MediaSkeleton } from "@/components/admin/media/media-skeleton";
import { MediaEmptyState } from "@/components/admin/media/media-empty-state";
import { MediaPagination } from "@/components/admin/media/media-pagination";
import { MediaUploadDialog } from "@/components/admin/media/media-upload-dialog";
import { MediaDetailPanel } from "@/components/admin/media/media-detail-panel";

/**
 * `/admin/media` — Media Library workspace.
 * List source: `GET /api/v1/admin/media` (server pagination + search).
 * Upload: `POST /api/v1/admin/media` (multipart/form-data).
 * There is NO delete endpoint — no delete UI is offered here.
 */
export function MediaWorkspace() {
  return (
    <Suspense fallback={<MediaWorkspaceFallback />}>
      <MediaWorkspaceInner />
    </Suspense>
  );
}

function MediaWorkspaceFallback() {
  return (
    <div className="space-y-6">
      <AdminPageHeader title="رسانه‌ها" description="بارگذاری و مدیریت تصاویر HelpDev" />
      <AdminLoadingState cards={0} rows={0} />
      <MediaSkeleton />
    </div>
  );
}

function MediaWorkspaceInner() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const query = parseAdminMediaListQuery(searchParams);

  const list = useAdminMediaList(query);
  const [uploadOpen, setUploadOpen] = useState(false);
  const [detailId, setDetailId] = useState<string | null>(null);

  const replaceQuery = useCallback(
    (patch: Partial<AdminMediaListQuery>) => {
      const next = mergeAdminMediaListQuery(query, patch);
      router.replace(buildAdminMediaListHref(next), { scroll: false });
    },
    [query, router],
  );

  const clearFilters = useCallback(() => {
    router.replace(buildAdminMediaListHref(DEFAULT_ADMIN_MEDIA_LIST_QUERY), { scroll: false });
  }, [router]);

  const handleUploaded = useCallback(() => {
    // Jump back to page 1 so the newest upload is visible immediately.
    if (query.page !== 1) {
      replaceQuery({ page: 1 });
    } else {
      list.reload();
    }
  }, [query.page, replaceQuery, list]);

  const filtered = isAdminMediaListFiltered(query);
  const matchingLabel =
    list.data != null
      ? `${formatNumberFa(list.data.totalCount)} رسانه${filtered ? " (با جستجوی فعلی)" : ""}`
      : undefined;

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="رسانه‌ها"
        description="بارگذاری و مدیریت تصاویر HelpDev"
        meta={matchingLabel}
      />

      <AdminPageSection>
        <div className="space-y-4">
          <MediaToolbar
            search={query.search}
            disabled={list.loading && !list.data}
            onSearchCommit={(search) => replaceQuery({ search })}
            onUploadClick={() => setUploadOpen(true)}
          />

          {list.refreshing ? (
            <p className="adm-subtle text-[11px]" role="status" aria-live="polite">
              در حال به‌روزرسانی فهرست...
            </p>
          ) : null}

          {list.loading && !list.data ? (
            <MediaSkeleton />
          ) : list.error && !list.data ? (
            <AdminErrorState error={list.error} onRetry={list.reload} />
          ) : list.data && list.data.items.length === 0 ? (
            <MediaEmptyState
              filtered={filtered}
              onClearFilters={filtered ? clearFilters : undefined}
              onUpload={() => setUploadOpen(true)}
            />
          ) : list.data ? (
            <>
              {list.error ? <AdminErrorState error={list.error} onRetry={list.reload} /> : null}
              <MediaGrid items={list.data.items} onItemClick={(item) => setDetailId(item.id)} />
              <MediaPagination
                page={list.data.page}
                pageSize={list.data.pageSize}
                totalCount={list.data.totalCount}
                totalPages={list.data.totalPages}
                disabled={list.refreshing}
                onPageChange={(page) => replaceQuery({ page })}
                onPageSizeChange={(pageSize) => replaceQuery({ pageSize })}
              />
            </>
          ) : null}
        </div>
      </AdminPageSection>

      <MediaUploadDialog
        open={uploadOpen}
        onClose={() => setUploadOpen(false)}
        onUploaded={handleUploaded}
      />

      <MediaDetailPanel id={detailId} onClose={() => setDetailId(null)} />
    </div>
  );
}
