"use client";

import { AdminIcon } from "@/components/admin/shared/admin-icons";
import type { AdminIconName } from "@/lib/admin/navigation";
import type { SeoAnalysisFinding } from "@/lib/admin/content/content-types";

type SeoFindingItemProps = {
  finding: SeoAnalysisFinding;
  /** Optional checklist label override (e.g. «عنوان سئو»). */
  compactLabel?: string;
};

export function SeoFindingItem({ finding, compactLabel }: SeoFindingItemProps) {
  const icon = findingIcon(finding);
  const title = compactLabel ?? finding.message.split(":")[0] ?? finding.message;

  return (
    <div className="rounded-lg border border-[var(--adm-border)] p-2.5">
      <div className="flex items-start gap-2">
        <span
          aria-hidden
          className={`mt-0.5 inline-flex h-4 w-4 flex-shrink-0 items-center justify-center rounded-full ${icon.toneClass}`}
        >
          <AdminIcon name={icon.name} size={11} />
        </span>
        <div className="min-w-0 flex-1 space-y-1">
          <p className="adm-text text-[12px] font-semibold">
            {title}
            {!compactLabel ? (
              <span className="adm-subtle ms-1.5 text-[10px] font-normal">
                ({finding.severityLabel})
              </span>
            ) : null}
          </p>
          <p className="adm-muted text-[11px] leading-5">{finding.message}</p>
          {finding.suggestion ? (
            <p className="text-[11px] leading-5 text-[var(--adm-info)]">
              <span className="font-semibold">پیشنهاد: </span>
              {finding.suggestion}
            </p>
          ) : null}
        </div>
      </div>
    </div>
  );
}

function findingIcon(finding: SeoAnalysisFinding): {
  name: AdminIconName;
  toneClass: string;
} {
  if (finding.passed) {
    return { name: "check", toneClass: "bg-[var(--adm-success-soft)] text-[var(--adm-success)]" };
  }
  if (finding.severity === "Error") {
    return { name: "close", toneClass: "bg-[var(--adm-danger-soft)] text-[var(--adm-danger)]" };
  }
  if (finding.severity === "Warning") {
    return { name: "bell", toneClass: "bg-[var(--adm-warning-soft)] text-[var(--adm-warning)]" };
  }
  return { name: "health", toneClass: "bg-[var(--adm-info-soft)] text-[var(--adm-info)]" };
}
