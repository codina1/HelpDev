"use client";

import Link from "next/link";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { formatNumberFa } from "@/lib/admin/dashboard/dashboard-mappers";
import { formatDateTimeFa, labelForContentStatus } from "@/lib/admin/content/content-mappers";
import { isKnownContentStatus } from "@/lib/admin/content/workflow/workflow-labels";
import { useSeoDashboard } from "@/lib/admin/seo/seo-hooks";
import type {
  SeoDashboardCriticalFindingRawDto,
  SeoDashboardRecentContentRawDto,
} from "@/lib/admin/seo/seo-types";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { AdminPageSection } from "@/components/admin/page/admin-page-section";
import { AdminStatCard } from "@/components/admin/page/admin-stat-card";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { AdminSurface } from "@/components/admin/page/admin-surface";
import { ContentStatusBadge } from "@/components/admin/content/list/content-status-badge";

export function SeoDashboardWorkspace() {
  const { data, loading, error, reload } = useSeoDashboard();

  if (loading && !data) {
    return (
      <div className="space-y-6">
        <AdminPageHeader title="تحلیل SEO" description="پوشش متادیتا و یافته‌های ذخیره‌شده در پایگاه داده" />
        <AdminLoadingState cards={4} rows={4} />
      </div>
    );
  }

  if (error) {
    return (
      <div className="space-y-6">
        <AdminPageHeader title="تحلیل SEO" description="پوشش متادیتا و یافته‌های ذخیره‌شده در پایگاه داده" />
        <AdminErrorState error={error} title="بارگذاری داشبورد سئو ناموفق بود" onRetry={reload} />
      </div>
    );
  }

  if (!data) {
    return (
      <div className="space-y-6">
        <AdminPageHeader title="تحلیل SEO" description="پوشش متادیتا و یافته‌های ذخیره‌شده در پایگاه داده" />
        <p className="adm-subtle text-center text-[13px]">داده‌ای برای نمایش وجود ندارد.</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="تحلیل SEO"
        description="آمار واقعی از محتوای ذخیره‌شده — بدون امتیاز، رتبه‌بندی یا تاریخچهٔ تحلیل."
      />

      <AdminPageSection title="پوشش محتوا">
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <AdminStatCard label="کل محتوا" value={formatNumberFa(data.totalContent)} icon="content" />
          <AdminStatCard
            label="منتشرشده"
            value={formatNumberFa(data.publishedContent)}
            icon="check"
            tone="success"
          />
        </div>
      </AdminPageSection>

      <AdminPageSection title="وضعیت متادیتای سئو">
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <AdminStatCard
            label="بدون عنوان سئو"
            value={formatNumberFa(data.missingSeoTitleCount)}
            icon="seo"
            tone="warning"
          />
          <AdminStatCard
            label="بدون توضیحات سئو"
            value={formatNumberFa(data.missingSeoDescriptionCount)}
            icon="seo"
            tone="warning"
          />
          <AdminStatCard
            label="بدون کانونیکال"
            value={formatNumberFa(data.missingCanonicalCount)}
            icon="flag"
            tone="neutral"
          />
        </div>
      </AdminPageSection>

      <AdminPageSection title="پوشش تصویر">
        <AdminStatCard
          label="بدون تصویر کاور"
          value={formatNumberFa(data.missingCoverImageCount)}
          icon="media"
          tone="warning"
        />
      </AdminPageSection>

      <AdminPageSection title="مسائل فنی">
        <AdminSurface padding="sm">
          <p className="adm-subtle text-[13px] leading-6">
            تاریخ آخرین تحلیل ذخیره‌شده:{" "}
            <span className="adm-text font-medium">
              {data.lastAnalysisTime
                ? formatDateTimeFa(data.lastAnalysisTime)
                : "— (تحلیل در نسخهٔ v1 ذخیره نمی‌شود)"}
            </span>
          </p>
        </AdminSurface>
      </AdminPageSection>

      <AdminPageSection title="یافته‌های مهم">
        {data.criticalFindings.length === 0 ? (
          <AdminSurface padding="sm">
            <p className="adm-subtle text-[13px]">مورد بحرانی بر اساس متادیتای ذخیره‌شده یافت نشد.</p>
          </AdminSurface>
        ) : (
          <CriticalFindingsTable rows={data.criticalFindings} />
        )}
      </AdminPageSection>

      <AdminPageSection title="محتوای اخیر">
        {data.recentContent.length === 0 ? (
          <AdminSurface padding="sm">
            <p className="adm-subtle text-[13px]">محتوایی ثبت نشده است.</p>
          </AdminSurface>
        ) : (
          <RecentSeoContentTable rows={data.recentContent} />
        )}
      </AdminPageSection>
    </div>
  );
}

function studioHref(contentId: string): string {
  return `${ADMIN_ROUTES.content}/${encodeURIComponent(contentId)}/edit`;
}

function CriticalFindingsTable({ rows }: { rows: SeoDashboardCriticalFindingRawDto[] }) {
  return (
    <AdminSurface padding="none" className="adm-scroll overflow-x-auto">
      <table className="adm-table w-full min-w-[36rem] text-[13px]">
        <thead>
          <tr>
            <th className="w-[36%]">عنوان</th>
            <th>پیام</th>
            <th className="w-[7.5rem]">عمل</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={`${row.contentId}-${row.issueCode}`}>
              <td>
                <span className="adm-text block max-w-[22rem] truncate font-semibold" title={row.title}>
                  {row.title}
                </span>
              </td>
              <td>
                <span className="inline-flex max-w-full rounded-md bg-[var(--adm-warning-soft)] px-2 py-1 text-[11px] font-medium leading-5 text-[var(--adm-warning)]">
                  {row.message}
                </span>
              </td>
              <td>
                <Link
                  href={studioHref(row.contentId)}
                  className="adm-btn adm-btn-outline adm-focus px-2.5 py-1 text-[11px]"
                >
                  استودیو
                </Link>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </AdminSurface>
  );
}

function RecentSeoContentTable({ rows }: { rows: SeoDashboardRecentContentRawDto[] }) {
  return (
    <AdminSurface padding="none" className="adm-scroll overflow-x-auto">
      <table className="adm-table w-full min-w-[40rem] text-[13px]">
        <thead>
          <tr>
            <th className="w-[32%]">عنوان</th>
            <th>وضعیت</th>
            <th>به‌روزرسانی</th>
            <th>متادیتا</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={row.contentId}>
              <td>
                <Link
                  href={studioHref(row.contentId)}
                  className="adm-link block max-w-[22rem] truncate font-semibold"
                  title={row.title}
                >
                  {row.title}
                </Link>
              </td>
              <td>
                {isKnownContentStatus(row.status) ? (
                  <ContentStatusBadge status={row.status} />
                ) : (
                  <span className="inline-flex rounded-md bg-[var(--adm-surface-3)] px-2 py-0.5 text-[11px] font-bold text-[var(--adm-text-muted)]">
                    {labelForContentStatus(row.status)}
                  </span>
                )}
              </td>
              <td className="adm-muted whitespace-nowrap text-[12px]">
                {formatDateTimeFa(row.updatedAtUtc)}
              </td>
              <td>
                <div className="flex flex-wrap gap-1.5">
                  <SeoMetaChip ok={!row.missingSeoTitle} okLabel="عنوان سئو" missingLabel="بدون عنوان سئو" />
                  <SeoMetaChip
                    ok={!row.missingSeoDescription}
                    okLabel="توضیحات"
                    missingLabel="بدون توضیحات"
                  />
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </AdminSurface>
  );
}

function SeoMetaChip({
  ok,
  okLabel,
  missingLabel,
}: {
  ok: boolean;
  okLabel: string;
  missingLabel: string;
}) {
  return (
    <span
      className={`inline-flex rounded-md px-2 py-0.5 text-[11px] font-semibold ${
        ok
          ? "bg-[var(--adm-success-soft)] text-[var(--adm-success)]"
          : "bg-[var(--adm-warning-soft)] text-[var(--adm-warning)]"
      }`}
    >
      {ok ? okLabel : missingLabel}
    </span>
  );
}
