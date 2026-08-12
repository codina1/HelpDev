"use client";

import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { formatNumberFa } from "@/lib/admin/dashboard/dashboard-mappers";
import { formatDateTimeFa, groupSeoAnalysisFindings } from "@/lib/admin/content/content-mappers";
import type {
  SeoAnalysisReport,
  SeoAnalysisStatus,
} from "@/lib/admin/content/content-types";
import { SeoChecklist } from "@/components/admin/content/seo/seo-checklist";
import { SeoFindingItem } from "@/components/admin/content/seo/seo-finding-item";

type SeoAnalysisPanelProps = {
  status: SeoAnalysisStatus;
  report: SeoAnalysisReport | null;
  error?: unknown;
  onAnalyze: () => void;
};

/**
 * SEO Analyzer — factual audit findings for the SAVED server version only.
 * No score, percentage, ranking prediction or AI wording.
 */
export function SeoAnalysisPanel({ status, report, error, onAnalyze }: SeoAnalysisPanelProps) {
  const isAnalyzing = status === "analyzing";
  const isStale = status === "stale";
  const hasReport = report !== null;
  const sections = hasReport ? groupSeoAnalysisFindings(report.findings) : [];

  return (
    <section className="space-y-3" aria-labelledby="seo-analysis-heading">
      <div className="flex items-center justify-between gap-2">
        <h3
          id="seo-analysis-heading"
          className="adm-text inline-flex items-center gap-1.5 text-[13px] font-bold"
        >
          <AdminIcon name="analytics" size={15} />
          تحلیل سئو
        </h3>
        <button
          type="button"
          onClick={onAnalyze}
          disabled={isAnalyzing}
          aria-label={hasReport ? "اجرای دوباره تحلیل سئو" : "اجرای تحلیل سئو"}
          className="adm-btn adm-btn-outline adm-focus inline-flex items-center gap-1.5 px-2.5 py-1 text-[11px]"
        >
          <AdminIcon name="analytics" size={13} />
          {isAnalyzing ? "در حال تحلیل…" : hasReport ? "تحلیل مجدد" : "تحلیل"}
        </button>
      </div>

      <p className="adm-subtle text-[11px]">
        تحلیل براساس آخرین نسخهٔ ذخیره‌شده — تغییرات ذخیره‌نشده در این تحلیل لحاظ نمی‌شوند.
      </p>

      <p role="status" aria-live="polite" className="sr-only">
        {isAnalyzing ? "در حال تحلیل سئو…" : ""}
      </p>

      {isAnalyzing ? (
        <div className="adm-subtle flex items-center gap-2 rounded-lg border border-[var(--adm-border)] bg-[var(--adm-surface-2)] p-3 text-[12px]">
          <span
            aria-hidden
            className="h-3.5 w-3.5 animate-spin rounded-full border-2 border-current border-t-transparent"
          />
          در حال تحلیل نسخهٔ ذخیره‌شده…
        </div>
      ) : null}

      {error ? (
        <AdminErrorState
          error={error}
          title="تحلیل سئو ناموفق بود"
          onRetry={onAnalyze}
          showHome={false}
        />
      ) : null}

      {!hasReport && !isAnalyzing && !error ? (
        <p className="adm-subtle rounded-lg border border-dashed border-[var(--adm-border)] p-3 text-center text-[12px]">
          برای مشاهده یافته‌های سئو، دکمهٔ «تحلیل» را بزنید.
        </p>
      ) : null}

      {hasReport ? (
        <div className="space-y-3">
          {isStale ? (
            <p className="rounded-lg border border-[var(--adm-warning-soft)] bg-[var(--adm-warning-soft)] px-3 py-2 text-[11px] font-semibold text-[var(--adm-warning)]">
              این نتیجه مربوط به نسخهٔ قبلی ذخیره‌شده است؛ برای به‌روزرسانی، دوباره تحلیل کنید.
            </p>
          ) : null}

          <p className="adm-subtle text-[11px]">زمان تحلیل: {formatDateTimeFa(report.analyzedAtUtc)}</p>

          <dl className="grid grid-cols-3 gap-2">
            <SummaryStat label="خطا" value={report.summary.errorCount} tone="danger" />
            <SummaryStat label="هشدار" value={report.summary.warningCount} tone="warning" />
            <SummaryStat label="اطلاعاتی" value={report.summary.infoCount} tone="neutral" />
          </dl>

          <div className="space-y-2">
            <h4 className="adm-text text-[12px] font-bold">چک‌لیست متادیتا</h4>
            <SeoChecklist findings={report.findings} />
          </div>

          {sections.map((section) => (
            <div key={section.key} className="space-y-1.5">
              <h4 className="adm-text text-[12px] font-bold">{section.label}</h4>
              <ul className="space-y-1.5">
                {section.findings.map((finding) => (
                  <li key={`${section.key}-${finding.ruleId}`}>
                    <SeoFindingItem finding={finding} />
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>
      ) : null}
    </section>
  );
}

function SummaryStat({
  label,
  value,
  tone,
}: {
  label: string;
  value: number;
  tone: "danger" | "warning" | "neutral";
}) {
  const toneClass =
    tone === "danger"
      ? "text-[var(--adm-danger)]"
      : tone === "warning"
        ? "text-[var(--adm-warning)]"
        : "adm-text";

  return (
    <div className="rounded-lg border border-[var(--adm-border)] bg-[var(--adm-surface-2)] p-2.5">
      <dt className="adm-subtle text-[11px]">{label}</dt>
      <dd className={`mt-0.5 text-[16px] font-black tabular-nums ${toneClass}`}>
        {formatNumberFa(value)}
      </dd>
    </div>
  );
}
