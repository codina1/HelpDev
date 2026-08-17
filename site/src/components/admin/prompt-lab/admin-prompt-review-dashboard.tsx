"use client";

import { Suspense, useCallback, useMemo, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { AdminPageSection } from "@/components/admin/page/admin-page-section";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { AdminPagination } from "@/components/admin/shared/admin-pagination";
import { AdminSurface } from "@/components/admin/page/admin-surface";
import { AdminPromptReviewTabs } from "@/components/admin/prompt-lab/admin-prompt-review-tabs";
import { AdminPromptReviewTable } from "@/components/admin/prompt-lab/admin-prompt-review-table";
import { AdminPromptRejectDialog } from "@/components/admin/prompt-lab/admin-prompt-reject-dialog";
import {
  useAdminPromptReviewActions,
  useAdminPromptReviewList,
} from "@/lib/admin/prompt-lab/admin-prompt-review-hooks";
import {
  buildAdminPromptReviewHref,
  mergeAdminPromptReviewQuery,
  parseAdminPromptReviewQuery,
} from "@/lib/admin/prompt-lab/admin-prompt-review-url-state";
import {
  ADMIN_PROMPT_REVIEW_TAB_LABELS,
  type AdminPromptReviewItem,
  type AdminPromptReviewPageSize,
  type AdminPromptReviewQuery,
} from "@/lib/admin/prompt-lab/admin-prompt-review-types";

/**
 * `/admin/prompts` — Admin review of writer library prompts.
 * List: GET /admin/prompts?status=Submitted|Approved|Rejected
 * Actions on pending: approve / reject with reason.
 */
export function AdminPromptReviewDashboard() {
  return (
    <Suspense fallback={<AdminPromptReviewFallback />}>
      <AdminPromptReviewDashboardInner />
    </Suspense>
  );
}

function AdminPromptReviewFallback() {
  return (
    <div className="space-y-6">
      <AdminPageHeader title="بازبینی پرامپت‌ها" description="تأیید یا رد پرامپت‌های ارسال‌شده" />
      <AdminLoadingState cards={0} rows={6} />
    </div>
  );
}

function AdminPromptReviewDashboardInner() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const query = useMemo(() => parseAdminPromptReviewQuery(searchParams), [searchParams]);
  const list = useAdminPromptReviewList(query);
  const actions = useAdminPromptReviewActions();
  const [rejectTarget, setRejectTarget] = useState<AdminPromptReviewItem | null>(null);

  const replaceQuery = useCallback(
    (patch: Partial<AdminPromptReviewQuery>) => {
      router.replace(buildAdminPromptReviewHref(mergeAdminPromptReviewQuery(query, patch)), {
        scroll: false,
      });
    },
    [query, router],
  );

  const handleApprove = useCallback(
    async (item: AdminPromptReviewItem) => {
      try {
        await actions.approve(item.id);
        list.reload();
      } catch {
        // Surfaced via actions.error
      }
    },
    [actions, list],
  );

  const handleRejectConfirm = useCallback(
    async (reason: string) => {
      if (!rejectTarget) return;
      try {
        await actions.reject(rejectTarget.id, reason);
        setRejectTarget(null);
        list.reload();
      } catch {
        // Surfaced via actions.error
      }
    },
    [actions, list, rejectTarget],
  );

  const pending = query.tab === "pending";

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="بازبینی پرامپت‌ها"
        description="پرامپت‌های ارسال‌شده را بررسی کنید. تأیید آن‌ها را عمومی می‌کند؛ رد با دلیل الزامی است."
        breadcrumbs={[
          { title: "Prompt Lab", href: "/admin/prompt-lab", current: false },
          { title: "بازبینی", current: true },
        ]}
      />

      <AdminPageSection title={ADMIN_PROMPT_REVIEW_TAB_LABELS[query.tab]}>
        <div className="space-y-4">
          <AdminPromptReviewTabs query={query} />

          {list.refreshing ? (
            <p className="adm-subtle text-[11px]" role="status" aria-live="polite">
              در حال به‌روزرسانی فهرست...
            </p>
          ) : null}

          {actions.error ? <AdminErrorState error={actions.error} showHome={false} /> : null}

          {list.loading && !list.data ? (
            <AdminLoadingState cards={0} rows={6} />
          ) : list.error && !list.data ? (
            <AdminErrorState error={list.error} onRetry={list.reload} />
          ) : list.data && list.data.items.length === 0 ? (
            <AdminSurface className="py-10 text-center">
              <p className="adm-text text-[15px] font-bold">موردی در این وضعیت نیست</p>
              <p className="adm-muted mt-2 text-[13px]">
                {pending
                  ? "پرامپت در انتظار بررسی وجود ندارد."
                  : "برای این زبانه پرامپتی ثبت نشده است."}
              </p>
            </AdminSurface>
          ) : list.data ? (
            <>
              {list.error ? <AdminErrorState error={list.error} onRetry={list.reload} /> : null}
              <AdminPromptReviewTable
                items={list.data.items}
                showActions={pending}
                busyId={actions.submittingId}
                onApprove={(item) => void handleApprove(item)}
                onReject={setRejectTarget}
              />
              <AdminPagination
                page={list.data.page}
                pageSize={list.data.pageSize}
                totalCount={list.data.totalCount}
                totalPages={list.data.totalPages}
                disabled={list.refreshing || Boolean(actions.submittingId)}
                ariaLabel="صفحه‌بندی بازبینی پرامپت‌ها"
                idPrefix="admin-prompt-review"
                onPageChange={(page) => replaceQuery({ page })}
                onPageSizeChange={(pageSize) =>
                  replaceQuery({ pageSize: pageSize as AdminPromptReviewPageSize })
                }
              />
            </>
          ) : null}
        </div>
      </AdminPageSection>

      <AdminPromptRejectDialog
        open={rejectTarget !== null}
        title={rejectTarget ? `رد «${rejectTarget.title}»` : "رد پرامپت"}
        disabled={Boolean(actions.submittingId)}
        onConfirm={(reason) => void handleRejectConfirm(reason)}
        onCancel={() => setRejectTarget(null)}
      />
    </div>
  );
}
