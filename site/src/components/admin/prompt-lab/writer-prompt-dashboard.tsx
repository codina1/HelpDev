"use client";

import { Suspense, useCallback, useMemo } from "react";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { formatNumberFa } from "@/lib/admin/dashboard/dashboard-mappers";
import {
  useWriterPromptList,
  useWriterPromptStats,
} from "@/lib/admin/prompt-lab/writer-prompt-hooks";
import {
  buildWriterPromptListHref,
  isWriterPromptListFiltered,
  mergeWriterPromptListQuery,
  parseWriterPromptListQuery,
} from "@/lib/admin/prompt-lab/writer-prompt-url-state";
import {
  DEFAULT_WRITER_PROMPT_LIST_QUERY,
  WRITER_PROMPT_NEW_PATH,
  type WriterPromptListQuery,
  type WriterPromptPageSize,
} from "@/lib/admin/prompt-lab/writer-prompt-types";
import type { ContentWorkspaceDefinition } from "@/lib/admin/content/factory";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { AdminPageSection } from "@/components/admin/page/admin-page-section";
import { AdminStatCard } from "@/components/admin/page/admin-stat-card";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { AdminPagination } from "@/components/admin/shared/admin-pagination";
import { WorkspaceHeader } from "@/components/admin/content/workspaces/workspace-header";
import { WriterPromptStatusLegend } from "@/components/admin/prompt-lab/writer-prompt-status-legend";
import { WriterPromptToolbar } from "@/components/admin/prompt-lab/writer-prompt-toolbar";
import { WriterPromptTable } from "@/components/admin/prompt-lab/writer-prompt-table";
import { WriterPromptEmptyState } from "@/components/admin/prompt-lab/writer-prompt-empty-state";

type WriterPromptDashboardProps = {
  workspace?: ContentWorkspaceDefinition;
  basePath?: string;
};

/**
 * Writer Prompt Studio — `/admin/prompt-lab` (and content workspace alias).
 * List: `GET /api/v1/writer/prompts`. Stats: derived totals from the same endpoint.
 */
export function WriterPromptDashboard({
  workspace,
  basePath = ADMIN_ROUTES.promptLab,
}: WriterPromptDashboardProps) {
  return (
    <Suspense fallback={<WriterPromptDashboardFallback workspace={workspace} />}>
      <WriterPromptDashboardInner workspace={workspace} basePath={basePath} />
    </Suspense>
  );
}

function WriterPromptDashboardFallback({
  workspace,
}: {
  workspace?: ContentWorkspaceDefinition;
}) {
  return (
    <div className="space-y-6">
      {workspace ? (
        <WorkspaceHeader workspace={workspace} />
      ) : (
        <AdminPageHeader
          title="Writer Prompt Studio"
          description="مدیریت پرامپت‌های شما در Prompt Lab"
        />
      )}
      <AdminLoadingState cards={4} rows={6} />
    </div>
  );
}

function WriterPromptDashboardInner({
  workspace,
  basePath,
}: Required<Pick<WriterPromptDashboardProps, "basePath">> &
  Pick<WriterPromptDashboardProps, "workspace">) {
  const router = useRouter();
  const searchParams = useSearchParams();
  const query = useMemo(() => parseWriterPromptListQuery(searchParams), [searchParams]);

  const list = useWriterPromptList(query);
  const statsState = useWriterPromptStats();

  const replaceQuery = useCallback(
    (patch: Partial<WriterPromptListQuery>) => {
      const next = mergeWriterPromptListQuery(query, patch);
      router.replace(buildWriterPromptListHref(next, basePath), { scroll: false });
    },
    [query, router, basePath],
  );

  const clearFilters = useCallback(() => {
    router.replace(buildWriterPromptListHref(DEFAULT_WRITER_PROMPT_LIST_QUERY, basePath), {
      scroll: false,
    });
  }, [router, basePath]);

  const filtered = isWriterPromptListFiltered(query);
  const statsLoading = statsState.loading && !statsState.stats;
  const statValue = (value: number | null | undefined) =>
    statsState.stats && value != null ? formatNumberFa(value) : "—";

  const matchingLabel =
    list.data != null
      ? `${formatNumberFa(list.data.totalCount)} نتیجه${filtered ? " (با فیلتر فعلی)" : ""}`
      : undefined;

  const createAction = (
    <Link
      href={WRITER_PROMPT_NEW_PATH}
      className="adm-btn adm-btn-primary adm-focus inline-flex items-center gap-1.5"
    >
      <AdminIcon name="plus" size={16} />
      ایجاد پرامپت
    </Link>
  );

  return (
    <div className="space-y-6">
      {workspace ? (
        <WorkspaceHeader workspace={workspace} meta={matchingLabel} primaryAction={createAction} />
      ) : (
        <AdminPageHeader
          title="Writer Prompt Studio"
          description="مدیریت پرامپت‌های شما — ایجاد، ارسال و پیگیری وضعیت"
          meta={matchingLabel}
          primaryAction={createAction}
        />
      )}

      <AdminPageSection title="آمار پرامپت‌ها">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <AdminStatCard
            label="کل پرامپت‌ها"
            icon="prompt"
            tone="info"
            value={statValue(statsState.stats?.total)}
            loading={statsLoading}
          />
          <AdminStatCard
            label="پیش‌نویس"
            icon="content"
            tone="warning"
            value={statValue(statsState.stats?.drafts)}
            loading={statsLoading}
          />
          <AdminStatCard
            label="در انتظار بررسی"
            icon="news"
            tone="neutral"
            value={statValue(statsState.stats?.pendingReview)}
            loading={statsLoading}
          />
          <AdminStatCard
            label="منتشرشده"
            icon="check"
            tone="success"
            value={statValue(statsState.stats?.published)}
            loading={statsLoading}
          />
        </div>
        {statsState.error ? (
          <AdminErrorState error={statsState.error} onRetry={statsState.reload} />
        ) : null}
        <p className="adm-subtle mt-2 text-[11px]">
          آمار از API فهرست نویسنده (<span dir="ltr">GET /writer/prompts</span>) با{" "}
          <span dir="ltr">total</span> هر وضعیت است — بدون endpoint جداگانه.
        </p>
      </AdminPageSection>

      <AdminPageSection title="فهرست پرامپت‌ها">
        <div className="space-y-4">
          <WriterPromptStatusLegend />
          <WriterPromptToolbar
            query={query}
            disabled={list.loading && !list.data}
            onStatusChange={(status) => replaceQuery({ status })}
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
            <WriterPromptEmptyState
              filtered={filtered}
              onClearFilters={filtered ? clearFilters : undefined}
            />
          ) : list.data ? (
            <>
              {list.error ? <AdminErrorState error={list.error} onRetry={list.reload} /> : null}
              <WriterPromptTable items={list.data.items} />
              <AdminPagination
                page={list.data.page}
                pageSize={list.data.pageSize}
                totalCount={list.data.totalCount}
                totalPages={list.data.totalPages}
                disabled={list.refreshing}
                ariaLabel="صفحه‌بندی فهرست پرامپت‌ها"
                idPrefix="writer-prompt"
                onPageChange={(page) => replaceQuery({ page })}
                onPageSizeChange={(pageSize) =>
                  replaceQuery({ pageSize: pageSize as WriterPromptPageSize })
                }
              />
            </>
          ) : null}
        </div>
      </AdminPageSection>
    </div>
  );
}
