"use client";

import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { analyzeContent } from "@/lib/admin/content/content-analyzer";
import { CONTENT_LIMITS, SEO_LIMITS } from "@/lib/admin/content/content-types";
import { formatNumberFa } from "@/lib/admin/dashboard/dashboard-mappers";

/**
 * Neutral, factual content checklist. Reports presence and real measurements
 * only — deliberately NO aggregate score, grade, or AI-derived judgement.
 */
export function ContentQualityPanel({
  title,
  description,
  body,
}: {
  title: string;
  description: string;
  body: string;
}) {
  const report = analyzeContent({ title, description, body });

  const checks: Array<{ ok: boolean; label: string }> = [
    { ok: report.title, label: "عنوان وارد شده است" },
    {
      ok: report.title && report.titleLength <= SEO_LIMITS.seoTitle,
      label: `طول عنوان مناسب است (${formatNumberFa(report.titleLength)} نویسه)`,
    },
    { ok: report.description, label: "خلاصه/توضیحات وارد شده است" },
    {
      ok: report.description && report.descriptionLength <= CONTENT_LIMITS.excerpt,
      label: `طول خلاصه مناسب است (${formatNumberFa(report.descriptionLength)} نویسه)`,
    },
    { ok: report.headings > 0, label: `دارای ساختار عنوان‌بندی (${formatNumberFa(report.headings)})` },
    { ok: report.bodyWords > 0, label: `متن دارای محتوا است (${formatNumberFa(report.bodyWords)} کلمه)` },
  ];

  return (
    <div className="space-y-3">
      <h3 className="adm-text text-[12px] font-bold">بررسی کیفیت محتوا</h3>
      <ul className="space-y-1.5">
        {checks.map((check) => (
          <li key={check.label} className="flex items-center gap-2 text-[12px]">
            <span
              aria-hidden
              className={`inline-flex h-4 w-4 items-center justify-center rounded-full ${
                check.ok
                  ? "bg-[var(--adm-success-soft)] text-[var(--adm-success)]"
                  : "bg-[var(--adm-warning-soft)] text-[var(--adm-warning)]"
              }`}
            >
              <AdminIcon name={check.ok ? "check" : "close"} size={11} />
            </span>
            <span className={check.ok ? "adm-muted" : "adm-text"}>{check.label}</span>
          </li>
        ))}
      </ul>
      <p className="adm-subtle text-[11px]">این موارد صرفاً بررسی‌های واقعی هستند؛ امتیاز سئو محاسبه نمی‌شود.</p>
    </div>
  );
}
