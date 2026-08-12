"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { useAdminContentDetail } from "@/lib/admin/content/content-hooks";
import {
  useContentRevisionDetail,
  useContentRevisions,
  useRestoreContentRevision,
} from "@/lib/admin/content/history/history-hooks";
import {
  ADMIN_CONTENT_REVISION_PAGE_SIZES,
  DEFAULT_CONTENT_REVISION_LIST_QUERY,
  type AdminContentRevisionPageSize,
  type ContentRevisionListQuery,
} from "@/lib/admin/content/history/history-types";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { AdminEmptyState } from "@/components/admin/feedback/admin-empty-state";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { AdminPagination } from "@/components/admin/shared/admin-pagination";
import { ContentDetailTabs } from "@/components/admin/content/details/content-detail-tabs";
import { RevisionList } from "@/components/admin/content/history/revision-list";
import { RevisionDetailPanel } from "@/components/admin/content/history/revision-detail-panel";
import { RevisionCompareView } from "@/components/admin/content/history/revision-compare-view";
import { RestoreConfirmDialog } from "@/components/admin/content/history/restore-confirm-dialog";

export function ContentHistoryWorkspace({ contentId }: { contentId: string }) {
  const router = useRouter();
  const [query, setQuery] = useState<ContentRevisionListQuery>(DEFAULT_CONTENT_REVISION_LIST_QUERY);
  const [selectedVersion, setSelectedVersion] = useState<number | null>(null);
  const [comparePair, setComparePair] = useState<{ newer: number; older: number } | null>(null);
  const [restoreOpen, setRestoreOpen] = useState(false);

  const { data: content, loading: contentLoading, error: contentError, reload: reloadContent } =
    useAdminContentDetail(contentId);
  const { data: listData, loading: listLoading, refreshing, error: listError, reload: reloadList } =
    useContentRevisions(contentId, query);
  const {
    data: selectedDetail,
    loading: detailLoading,
    error: detailError,
    reload: reloadDetail,
  } = useContentRevisionDetail(contentId, selectedVersion);
  const newerVersion = comparePair?.newer ?? null;
  const olderVersion = comparePair?.older ?? null;
  const { data: newerDetail, loading: newerLoading } = useContentRevisionDetail(
    contentId,
    newerVersion,
  );
  const { data: olderDetail, loading: olderLoading } = useContentRevisionDetail(
    contentId,
    olderVersion,
  );
  const { restore, submitting: restoring, error: restoreError } = useRestoreContentRevision();

  useEffect(() => {
    if (!listData?.items.length) return;
    if (selectedVersion != null && listData.items.some((i) => i.versionNumber === selectedVersion)) {
      return;
    }
    setSelectedVersion(listData.items[0].versionNumber);
  }, [listData, selectedVersion]);

  const olderCompareVersion =
    selectedVersion != null && selectedVersion > 1 ? selectedVersion - 1 : null;

  const handleRestore = useCallback(
    async (changeReason: string | null) => {
      if (selectedVersion == null) return;
      await restore(contentId, selectedVersion, { changeReason });
      setRestoreOpen(false);
      reloadList();
      reloadContent();
      router.refresh();
    },
    [contentId, selectedVersion, restore, reloadList, reloadContent, router],
  );

  const compareBusy = comparePair != null && (newerLoading || olderLoading);

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="تاریخچه محتوا"
        description={content?.title ?? "نسخه‌های ذخیره‌شده این محتوا"}
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

      {content ? <ContentDetailTabs id={content.id} active="history" /> : null}

      {contentLoading ? (
        <AdminLoadingState cards={0} rows={4} />
      ) : contentError ? (
        <AdminErrorState error={contentError} onRetry={reloadContent} />
      ) : !content ? (
        <AdminEmptyState
          icon="content"
          title="محتوا یافت نشد"
          description="این محتوا وجود ندارد یا به آن دسترسی ندارید."
        />
      ) : (
        <>
          <div className="grid grid-cols-1 gap-4 lg:grid-cols-[minmax(240px,320px)_1fr]">
            <div className="space-y-3">
              <RevisionList
                items={listData?.items ?? []}
                loading={listLoading}
                refreshing={refreshing}
                error={listError}
                selectedVersion={selectedVersion}
                onSelect={(version) => {
                  setSelectedVersion(version);
                  setComparePair(null);
                }}
                onRetry={reloadList}
              />
              {listData && listData.totalCount > 0 ? (
                <AdminPagination
                  page={listData.page}
                  pageSize={listData.pageSize}
                  totalCount={listData.totalCount}
                  totalPages={listData.totalPages}
                  disabled={listLoading || refreshing}
                  pageSizeOptions={ADMIN_CONTENT_REVISION_PAGE_SIZES}
                  ariaLabel="صفحه‌بندی تاریخچه"
                  idPrefix="admin-content-history"
                  onPageChange={(page) => setQuery((prev) => ({ ...prev, page }))}
                  onPageSizeChange={(pageSize) =>
                    setQuery((prev) => ({
                      ...prev,
                      page: 1,
                      pageSize: pageSize as AdminContentRevisionPageSize,
                    }))
                  }
                />
              ) : null}
            </div>

            <div className="space-y-4">
              {comparePair && newerDetail && olderDetail && !compareBusy ? (
                <>
                  <div className="flex flex-wrap items-center justify-between gap-2">
                    <p className="adm-muted text-[12px]">
                      مقایسه نسخه {comparePair.newer} با {comparePair.older}
                    </p>
                    <button
                      type="button"
                      className="adm-btn adm-btn-ghost adm-focus text-[12px]"
                      onClick={() => setComparePair(null)}
                    >
                      بستن مقایسه
                    </button>
                  </div>
                  <RevisionCompareView
                    leftLabel={`نسخه ${comparePair.newer} (جدیدتر)`}
                    rightLabel={`نسخه ${comparePair.older} (قدیمی‌تر)`}
                    left={newerDetail.snapshot}
                    right={olderDetail.snapshot}
                  />
                </>
              ) : compareBusy ? (
                <AdminLoadingState cards={1} rows={6} />
              ) : (
                <RevisionDetailPanel
                  detail={selectedDetail}
                  loading={detailLoading}
                  error={detailError}
                  onRetry={reloadDetail}
                  restoreDisabled={restoring}
                  compareWithOlderDisabled={olderCompareVersion == null}
                  onCompareWithOlder={() => {
                    if (selectedVersion == null || olderCompareVersion == null) return;
                    setComparePair({ newer: selectedVersion, older: olderCompareVersion });
                  }}
                  onRestore={() => setRestoreOpen(true)}
                />
              )}

              {restoreError ? <AdminErrorState error={restoreError} showHome={false} /> : null}
            </div>
          </div>

          {restoreOpen && selectedVersion != null ? (
            <RestoreConfirmDialog
              versionNumber={selectedVersion}
              disabled={restoring}
              onConfirm={handleRestore}
              onCancel={() => setRestoreOpen(false)}
            />
          ) : null}
        </>
      )}
    </div>
  );
}
