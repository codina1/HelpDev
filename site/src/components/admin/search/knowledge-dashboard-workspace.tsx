"use client";

import { useCallback, useEffect, useState } from "react";
import { useAuth } from "@/components/auth";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { AdminPageSection } from "@/components/admin/page/admin-page-section";
import {
  fetchKnowledgeDashboard,
  type KnowledgeDashboardDto,
} from "@/lib/api/search";
import { formatDateTimeFa } from "@/lib/admin/content/content-mappers";

const SOURCE_FILTERS = [
  { value: "all", label: "همه" },
  { value: "content", label: "مقالات" },
  { value: "course", label: "دوره‌ها" },
  { value: "lesson", label: "درس‌ها" },
  { value: "tool", label: "ابزارها" },
  { value: "prompt", label: "پرامپت‌ها" },
] as const;

export function KnowledgeDashboardWorkspace() {
  const { token } = useAuth();
  const [data, setData] = useState<KnowledgeDashboardDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);
  const [sourceFilter, setSourceFilter] = useState<string>("all");

  const load = useCallback(() => {
    if (!token) {
      setError(new Error("برای مشاهده داشبورد دانش باید وارد شوید."));
      setLoading(false);
      return;
    }

    const controller = new AbortController();
    setLoading(true);
    setError(null);
    fetchKnowledgeDashboard(token, sourceFilter === "all" ? undefined : sourceFilter, controller.signal)
      .then((dto) => {
        setData(dto);
        setLoading(false);
      })
      .catch((err) => {
        if (controller.signal.aborted) return;
        setError(err);
        setLoading(false);
      });

    return () => controller.abort();
  }, [token, sourceFilter]);

  useEffect(() => {
    return load();
  }, [load]);

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="دانش جستجو"
        description="وضعیت ایندکس معنایی HelpDev — بدون نمایش بردارها یا متن خام"
      />

      <div className="flex flex-wrap gap-2">
        {SOURCE_FILTERS.map((filter) => (
          <button
            key={filter.value}
            type="button"
            onClick={() => setSourceFilter(filter.value)}
            className={`rounded-md border px-3 py-1.5 text-[12px] ${
              sourceFilter === filter.value
                ? "border-[var(--adm-accent)] bg-[var(--adm-accent-soft)] adm-text font-semibold"
                : "border-[var(--adm-border)] adm-subtle"
            }`}
          >
            {filter.label}
          </button>
        ))}
      </div>

      {loading && !data ? <AdminLoadingState cards={3} rows={4} /> : null}
      {error ? (
        <AdminErrorState error={error} title="بارگذاری داشبورد دانش ناموفق بود" onRetry={load} />
      ) : null}

      {!loading && !error && data ? (
        <>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
            <Stat label="اسناد ایندکس‌شده" value={data.indexedDocuments} />
            <Stat label="قطعه‌ها" value={data.totalChunks} />
            <Stat label="منابع معنایی" value={data.indexedSources} />
            <Stat label="ناموفق" value={data.failedSources} tone="danger" />
          </div>

          <AdminPageSection title="ایندکس‌های اخیر">
            <StatusTable rows={data.recentIndexed} empty="هنوز منبعی ایندکس نشده است." />
          </AdminPageSection>

          <AdminPageSection title="خطاهای ایندکس">
            <StatusTable rows={data.recentFailures} empty="خطای ایندکسی ثبت نشده است." />
          </AdminPageSection>
        </>
      ) : null}
    </div>
  );
}

function Stat({
  label,
  value,
  tone = "neutral",
}: {
  label: string;
  value: number;
  tone?: "neutral" | "danger";
}) {
  return (
    <div className="rounded-lg border border-[var(--adm-border)] bg-[var(--adm-surface)] p-4">
      <p className="adm-subtle text-[11px]">{label}</p>
      <p
        className={`mt-1 text-[22px] font-bold ${
          tone === "danger" ? "text-[var(--adm-danger)]" : "adm-text"
        }`}
      >
        {value}
      </p>
    </div>
  );
}

function StatusTable({
  rows,
  empty,
}: {
  rows: KnowledgeDashboardDto["recentIndexed"];
  empty: string;
}) {
  if (rows.length === 0) {
    return <p className="adm-subtle text-[13px]">{empty}</p>;
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[520px] text-start text-[12px]">
        <thead className="adm-subtle border-b border-[var(--adm-border)]">
          <tr>
            <th className="px-2 py-2 font-semibold">نوع</th>
            <th className="px-2 py-2 font-semibold">شناسه</th>
            <th className="px-2 py-2 font-semibold">وضعیت</th>
            <th className="px-2 py-2 font-semibold">قطعه</th>
            <th className="px-2 py-2 font-semibold">به‌روزرسانی</th>
            <th className="px-2 py-2 font-semibold">کد خطا</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={`${row.sourceType}-${row.sourceId}`} className="border-b border-[var(--adm-border)]">
              <td className="px-2 py-2" dir="ltr">
                {row.sourceType}
              </td>
              <td className="px-2 py-2 font-mono text-[11px]" dir="ltr">
                {row.sourceId.slice(0, 8)}…
              </td>
              <td className="px-2 py-2">{row.status}</td>
              <td className="px-2 py-2">{row.chunkCount}</td>
              <td className="px-2 py-2">{formatDateTimeFa(row.updatedAtUtc)}</td>
              <td className="px-2 py-2" dir="ltr">
                {row.failureCode ?? "—"}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
