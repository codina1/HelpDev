"use client";

import type { SeoAnalysisFinding } from "@/lib/admin/content/content-types";
import { SeoFindingItem } from "@/components/admin/content/seo/seo-finding-item";

const CHECKLIST_FIELDS: Array<{ field: string; label: string }> = [
  { field: "seoTitle", label: "عنوان سئو" },
  { field: "seoDescription", label: "توضیحات سئو" },
  { field: "canonicalUrl", label: "نشانی کانونیکال" },
  { field: "coverImage", label: "تصویر کاور" },
  { field: "focusKeyword", label: "کلمه کلیدی" },
];

type SeoChecklistProps = {
  findings: SeoAnalysisFinding[];
};

/**
 * Compact checklist derived from audit findings (saved server version only).
 * No score badge — only factual rule outcomes.
 */
export function SeoChecklist({ findings }: SeoChecklistProps) {
  const byField = new Map<string, SeoAnalysisFinding>();
  for (const finding of findings) {
    if (finding.field) {
      byField.set(finding.field, finding);
    }
  }

  return (
    <ul className="space-y-1.5" aria-label="چک‌لیست سئو">
      {CHECKLIST_FIELDS.map(({ field, label }) => {
        const finding = byField.get(field);
        if (!finding) {
          return (
            <li
              key={field}
              className="adm-subtle flex items-center gap-2 rounded-lg border border-dashed border-[var(--adm-border)] px-2.5 py-2 text-[11px]"
            >
              <span aria-hidden>—</span>
              <span>{label}</span>
              <span className="ms-auto text-[10px]">پس از تحلیل</span>
            </li>
          );
        }

        return (
          <li key={field}>
            <SeoFindingItem finding={finding} compactLabel={label} />
          </li>
        );
      })}
    </ul>
  );
}
