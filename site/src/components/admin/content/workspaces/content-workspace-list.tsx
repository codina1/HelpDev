"use client";

import { Suspense, useCallback, useMemo, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { formatNumberFa } from "@/lib/admin/dashboard/dashboard-mappers";
import {
  useAdminContentList,
  usePublishContent,
} from "@/lib/admin/content/content-hooks";
import {
  buildAdminContentListHref,
  mergeAdminContentListQuery,
  parseAdminContentListQuery,
} from "@/lib/admin/content/content-url-state";
import {
  DEFAULT_ADMIN_CONTENT_LIST_QUERY,
  type AdminContentListItem,
  type AdminContentListQuery,
  type AdminContentPageSize,
} from "@/lib/admin/content/content-types";
import type { ContentWorkspaceDefinition } from "@/lib/admin/content/factory";
import { AdminPageSection } from "@/components/admin/page/admin-page-section";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { AdminPagination } from "@/components/admin/shared/admin-pagination";
import { ContentToolbar } from "@/components/admin/content/list/content-toolbar";
import { ContentTable } from "@/components/admin/content/list/content-table";
import { ContentBulkToolbar } from "@/components/admin/content/list/content-bulk-toolbar";
import { ContentStatusLegend } from "@/components/admin/content/list/content-status-legend";
import { WorkspaceHeader } from "@/components/admin/content/workspaces/workspace-header";
import { WorkspaceEmptyState } from "@/components/admin/content/workspaces/workspace-empty-state";
import { WorkspaceStats } from "@/components/admin/content/workspaces/workspace-stats";

type ContentWorkspaceListProps = {
  workspace: ContentWorkspaceDefinition;
};

/**
 * Typed content list for a workspace — reuses admin content list API with locked type.
 */
export function ContentWorkspaceList({ workspace }: ContentWorkspaceListProps) {
  return (
    <Suspense fallback={<WorkspaceListFallback workspace={workspace} />}>
      <ContentWorkspaceListInner workspace={workspace} />
    </Suspense>
  );
}

function WorkspaceListFallback({ workspace }: { workspace: ContentWorkspaceDefinition }) {
  return (
    <div className="space-y-6">
      <WorkspaceHeader workspace={workspace} />
      <AdminLoadingState cards={3} rows={6} />
    </div>
  );
}

function ContentWorkspaceListInner({ workspace }: ContentWorkspaceListProps) {
  const router = useRouter();
  const searchParams = useSearchParams();

  const query = useMemo(() => {
    const parsed = parseAdminContentListQuery(searchParams);
    if (workspace.contentType === "none") {
      return parsed;
    }
    return { ...parsed, type: workspace.contentType };
  }, [searchParams, workspace.contentType]);

  const list = useAdminContentList(
    workspace.contentType === "none"
      ? query
      : { ...query, type: workspace.contentType },
  );
  const publish = usePublishContent();
  const [publishingId, setPublishingId] = useState<string | null>(null);
  const [lastPublishId, setLastPublishId] = useState<string | null>(null);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  const replaceQuery = useCallback(
    (patch: Partial<AdminContentListQuery>) => {
      const lockedType = workspace.contentType === "none" ? patch.type : workspace.contentType;
      const next = mergeAdminContentListQuery(query, {
        ...patch,
        type: lockedType ?? query.type,
      });
      router.replace(buildAdminContentListHref(next, workspace.listHref), { scroll: false });
    },
    [query, router, workspace.contentType, workspace.listHref],
  );

  const clearFilters = useCallback(() => {
    router.replace(
      buildAdminContentListHref(
        {
          ...DEFAULT_ADMIN_CONTENT_LIST_QUERY,
          type: workspace.contentType === "none" ? "all" : workspace.contentType,
        },
        workspace.listHref,
      ),
    );
  }, [router, workspace.contentType, workspace.listHref]);

  const handlePublish = useCallback(
    async (item: AdminContentListItem) => {
      setPublishingId(item.id);
      setLastPublishId(item.id);
      publish.reset();
      try {
        await publish.run(item.id);
        list.reload();
      } catch {
        // Error on row
      } finally {
        setPublishingId(null);
      }
    },
    [publish, list],
  );

  const filtered =
    query.search.trim() !== "" || query.status !== "all";
  const matchingLabel =
    list.data != null ? `${formatNumberFa(list.data.totalCount)} مورد` : undefined;

  return (
    <div className="space-y-6">
      <WorkspaceHeader workspace={workspace} meta={matchingLabel} />
      <WorkspaceStats
        workspace={workspace}
        matchingCount={list.data?.totalCount ?? null}
        loading={list.loading && !list.data}
      />

      <AdminPageSection title="فهرست">
        <div className="space-y-4">
          <ContentStatusLegend />
          <ContentToolbar
            query={query}
            disabled={list.loading && !list.data}
            hideTypeFilter
            onSearchCommit={(search) => replaceQuery({ search })}
            onStatusChange={(status) => replaceQuery({ status })}
            onTypeChange={() => undefined}
          />
          <ContentBulkToolbar
            selectedCount={selectedIds.size}
            onClear={() => setSelectedIds(new Set())}
          />

          {list.loading && !list.data ? (
            <AdminLoadingState cards={0} rows={6} />
          ) : list.error && !list.data ? (
            <AdminErrorState error={list.error} onRetry={list.reload} />
          ) : list.data && list.data.items.length === 0 ? (
            <WorkspaceEmptyState
              workspace={workspace}
              filtered={filtered}
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
