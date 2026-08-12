"use client";

import { Suspense, useCallback, useState } from "react";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { formatNumberFa } from "@/lib/admin/dashboard/dashboard-mappers";
import {
  useAdminContentList,
  useContentStats,
  usePublishContent,
} from "@/lib/admin/content/content-hooks";
import {
  buildAdminContentListHref,
  isAdminContentListFiltered,
  mergeAdminContentListQuery,
  parseAdminContentListQuery,
} from "@/lib/admin/content/content-url-state";
import {
  DEFAULT_ADMIN_CONTENT_LIST_QUERY,
  type AdminContentListItem,
  type AdminContentListQuery,
  type AdminContentPageSize,
} from "@/lib/admin/content/content-types";
import { useAuth } from "@/components/auth";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { AdminPageSection } from "@/components/admin/page/admin-page-section";
import { AdminStatCard } from "@/components/admin/page/admin-stat-card";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { AdminPagination } from "@/components/admin/shared/admin-pagination";
import { ContentToolbar } from "@/components/admin/content/list/content-toolbar";
import { ContentTable } from "@/components/admin/content/list/content-table";
import { ContentEmptyState } from "@/components/admin/content/list/content-empty-state";
import { ContentBulkToolbar } from "@/components/admin/content/list/content-bulk-toolbar";
import { ContentStatusLegend } from "@/components/admin/content/list/content-status-legend";

/**
 * `/admin/content` — Content CMS workspace.
 * List source: `GET /api/v1/admin/content` (server pagination + filters).
 * Stats source: Admin Dashboard aggregates (not derived from the current page).
 */
export function ContentDashboard() {
  return (
    <Suspense fallback={<ContentDashboardFallback />}>
      <ContentDashboardInner />
    </Suspense>
  );
}

function ContentDashboardFallback() {
  return (
    <div className="space-y-6">
      <AdminPageHeader title="مدیریت محتوا" description="ایجاد، ویرایش و مدیریت محتوای HelpDev" />
      <AdminLoadingState cards={4} rows={6} />
    </div>
  );
}

function ContentDashboardInner() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { user } = useAuth();
  const query = parseAdminContentListQuery(searchParams);

  const list = useAdminContentList(query);
  const statsState = useContentStats();
  const publish = usePublishContent();
  const [publishingId, setPublishingId] = useState<string | null>(null);
  const [lastPublishId, setLastPublishId] = useState<string | null>(null);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  const replaceQuery = useCallback(
    (patch: Partial<AdminContentListQuery>) => {
      const next = mergeAdminContentListQuery(query, patch);
      router.replace(buildAdminContentListHref(next), { scroll: false });
    },
    [query, router],
  );

  const clearFilters = useCallback(() => {
    router.replace(buildAdminContentListHref(DEFAULT_ADMIN_CONTENT_LIST_QUERY), {
      scroll: false,
    });
  }, [router]);

  const handlePublish = useCallback(
    async (item: AdminContentListItem) => {
      setPublishingId(item.id);
      setLastPublishId(item.id);
      publish.reset();
      try {
        await publish.run(item.id);
        if (
          query.status === "Draft" &&
          list.data &&
          list.data.items.length === 1 &&
          query.page > 1
        ) {
          replaceQuery({ page: query.page - 1 });
        } else {
          list.reload();
        }
        statsState.reload();
      } catch {
        // Error is surfaced via publish.error on the active row.
      } finally {
        setPublishingId(null);
      }
    },
    [publish, query.status, query.page, list, replaceQuery, statsState],
  );

  const filtered = isAdminContentListFiltered(query);
  const writerScoped = user?.role === "Writer";
  const statsLoading = statsState.loading && !statsState.stats;
  const statValue = (value: number | null | undefined) =>
    statsState.stats && value != null ? formatNumberFa(value) : "—";

  const matchingLabel =
    list.data != null
      ? `${formatNumberFa(list.data.totalCount)} نتیجه${filtered ? " (با فیلتر فعلی)" : ""}`
      : undefined;

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="مدیریت محتوا"
        description="ایجاد، ویرایش و مدیریت محتوای HelpDev"
        meta={matchingLabel}
        primaryAction={
          <Link
            href={ADMIN_ROUTES.contentArticlesNew}
            className="adm-btn adm-btn-primary adm-focus inline-flex items-center gap-1.5"
          >
            <AdminIcon name="plus" size={16} />
            مقاله جدید
          </Link>
        }
      />

      <AdminPageSection title="آمار محتوا">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <AdminStatCard
            label="کل محتوا"
            icon="content"
            tone="info"
            value={statValue(statsState.stats?.total)}
            loading={statsLoading}
          />
          <AdminStatCard
            label="منتشرشده"
            icon="check"
            tone="success"
            value={statValue(statsState.stats?.published)}
            loading={statsLoading}
          />
          <AdminStatCard
            label="پیش‌نویس"
            icon="content"
            tone="warning"
            value={statValue(statsState.stats?.draft)}
            loading={statsLoading}
          />
          <AdminStatCard
            label="نتایج فیلتر فعلی"
            icon="news"
            tone="neutral"
            value={list.data != null ? formatNumberFa(list.data.totalCount) : "—"}
            loading={list.loading && !list.data}
          />
        </div>
        <p className="adm-subtle mt-2 text-[11px]">
          آمار کل/منتشرشده/پیش‌نویس از داشبورد ادمین است؛ «نتایج فیلتر فعلی» همان{" "}
          <span dir="ltr">totalCount</span> پاسخ فهرست است — نه شمارش صفحه جاری.
        </p>
      </AdminPageSection>

      <AdminPageSection title="فهرست محتوا">
        <div className="space-y-4">
          <ContentStatusLegend />
          <ContentToolbar
            query={query}
            disabled={list.loading && !list.data}
            onSearchCommit={(search) => replaceQuery({ search })}
            onStatusChange={(status) => replaceQuery({ status })}
            onTypeChange={(type) => replaceQuery({ type })}
          />
          <ContentBulkToolbar
            selectedCount={selectedIds.size}
            onClear={() => setSelectedIds(new Set())}
          />

          {list.refreshing ? (
            <p className="adm-subtle text-[11px]" role="status" aria-live="polite">
              در حال به‌روزرسانی فهرست...
            </p>
          ) : null}

          {list.loading && !list.data ? (
            <AdminLoadingState cards={0} rows={6} />
          ) : list.error && !list.data ? (
            <AdminErrorState error={list.error} onRetry={list.reload} />
          ) : list.data && list.data.items.length === 0 ? (
            <ContentEmptyState
              filtered={filtered}
              writerScoped={writerScoped}
              onClearFilters={filtered ? clearFilters : undefined}
            />
          ) : list.data ? (
            <>
              {list.error ? <AdminErrorState error={list.error} onRetry={list.reload} /> : null}
              <ContentTable
                items={list.data.items}
                publishingId={publishingId}
                publishError={lastPublishId ? publish.error : undefined}
                lastPublishId={lastPublishId}
                onPublish={handlePublish}
                selectedIds={selectedIds}
                onToggleSelect={(id) => {
                  setSelectedIds((prev) => {
                    const next = new Set(prev);
                    if (next.has(id)) next.delete(id);
                    else next.add(id);
                    return next;
                  });
                }}
                onToggleSelectAll={() => {
                  const pageIds = list.data!.items.map((item) => item.id);
                  setSelectedIds((prev) => {
                    const allSelected = pageIds.every((id) => prev.has(id));
                    if (allSelected) {
                      const next = new Set(prev);
                      pageIds.forEach((id) => next.delete(id));
                      return next;
                    }
                    const next = new Set(prev);
                    pageIds.forEach((id) => next.add(id));
                    return next;
                  });
                }}
              />
              <AdminPagination
                page={list.data.page}
                pageSize={list.data.pageSize}
                totalCount={list.data.totalCount}
                totalPages={list.data.totalPages}
                disabled={list.refreshing || publish.submitting}
                onPageChange={(page) => replaceQuery({ page })}
                onPageSizeChange={(pageSize) =>
                  replaceQuery({ pageSize: pageSize as AdminContentPageSize })
                }
              />
            </>
          ) : null}
        </div>
      </AdminPageSection>
    </div>
  );
}
