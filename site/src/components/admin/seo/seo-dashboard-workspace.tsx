"use client";

import Link from "next/link";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { formatNumberFa } from "@/lib/admin/dashboard/dashboard-mappers";
import { formatDateTimeFa } from "@/lib/admin/content/content-mappers";
import { useSeoDashboard } from "@/lib/admin/seo/seo-hooks";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { AdminPageSection } from "@/components/admin/page/admin-page-section";
import { AdminStatCard } from "@/components/admin/page/admin-stat-card";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { AdminSurface } from "@/components/admin/page/admin-surface";

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
        <p className="adm-subtle text-[12px]">
          تاریخ آخرین تحلیل ذخیره‌شده:{" "}
          {data.lastAnalysisTime ? formatDateTimeFa(data.lastAnalysisTime) : "— (تحلیل در نسخهٔ v1 ذخیره نمی‌شود)"}
        </p>
      </AdminPageSection>

      <AdminPageSection title="یافته‌های مهم">
        {data.criticalFindings.length === 0 ? (
          <p className="adm-subtle text-[12px]">مورد بحرانی بر اساس متادیتای ذخیره‌شده یافت نشد.</p>
        ) : (
          <AdminSurface padding="none" className="overflow-x-auto">
            <table className="adm-table w-full min-w-[32rem] text-[12px]">
              <thead>
                <tr>
                  <th className="text-start">عنوان</th>
                  <th className="text-start">پیام</th>
                  <th className="text-start">عمل</th>
                </tr>
              </thead>
              <tbody>
                {data.criticalFindings.map((row) => (
                  <tr key={`${row.contentId}-${row.issueCode}`}>
                    <td>{row.title}</td>
                    <td className="adm-muted">{row.message}</td>
                    <td>
                      <Link
                        href={`${ADMIN_ROUTES.content}/${encodeURIComponent(row.contentId)}/edit`}
                        className="adm-link text-[11px] font-semibold"
                      >
                        استودیو
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </AdminSurface>
        )}
      </AdminPageSection>

      <AdminPageSection title="محتوای اخیر">
        {data.recentContent.length === 0 ? (
          <p className="adm-subtle text-[12px]">محتوایی ثبت نشده است.</p>
        ) : (
          <AdminSurface padding="none" className="overflow-x-auto">
            <table className="adm-table w-full min-w-[36rem] text-[12px]">
              <thead>
                <tr>
                  <th className="text-start">عنوان</th>
                  <th className="text-start">وضعیت</th>
                  <th className="text-start">به‌روزرسانی</th>
                  <th className="text-start">متادیتا</th>
                </tr>
              </thead>
              <tbody>
                {data.recentContent.map((row) => (
                  <tr key={row.contentId}>
                    <td>
                      <Link
                        href={`${ADMIN_ROUTES.content}/${encodeURIComponent(row.contentId)}/edit`}
                        className="adm-link font-semibold"
                      >
                        {row.title}
                      </Link>
                    </td>
                    <td className="adm-muted">{row.status}</td>
                    <td className="adm-muted">{formatDateTimeFa(row.updatedAtUtc)}</td>
                    <td className="adm-muted text-[11px]">
                      {row.missingSeoTitle ? "بدون عنوان سئو" : "عنوان ✓"}
                      {" · "}
                      {row.missingSeoDescription ? "بدون توضیحات" : "توضیحات ✓"}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </AdminSurface>
        )}
      </AdminPageSection>
    </div>
  );
}
