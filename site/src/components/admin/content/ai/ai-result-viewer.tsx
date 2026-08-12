"use client";

import { formatDateTimeFa } from "@/lib/admin/content/content-mappers";
import type { ContentAiResult } from "@/lib/admin/content/content-types";

type AiResultViewerProps = {
  result: ContentAiResult;
};

/**
 * Displays AI suggestion text for human review/copy.
 * Does not provide an Apply/Replace button — editors must copy manually.
 */
export function AiResultViewer({ result }: AiResultViewerProps) {
  return (
    <section
      aria-labelledby="ai-result-heading"
      className="space-y-2 rounded-lg border border-[var(--adm-border)] bg-[var(--adm-surface)] p-3"
    >
      <div className="flex flex-wrap items-baseline justify-between gap-2">
        <h3 id="ai-result-heading" className="adm-text text-[13px] font-bold">
          نتیجه پیشنهاد
        </h3>
        <p className="adm-subtle text-[11px]">
          {result.model ? (
            <span dir="ltr" className="font-mono">
              {result.model}
            </span>
          ) : null}
          {result.createdAtUtc ? ` · ${formatDateTimeFa(result.createdAtUtc)}` : null}
        </p>
      </div>
      <p className="adm-subtle text-[11px]">
        وظیفه: <span dir="ltr">{result.taskType}</span>
        {result.provider ? (
          <>
            {" "}
            · ارائه‌دهنده: <span dir="ltr">{result.provider}</span>
          </>
        ) : null}
      </p>
      <pre
        dir="auto"
        className="adm-text max-h-80 overflow-auto whitespace-pre-wrap rounded-md border border-[var(--adm-border)] bg-[var(--adm-surface-2)] p-3 text-[12px] leading-6"
      >
        {result.generatedText}
      </pre>
      <p className="adm-subtle text-[11px]">
        این متن به‌صورت خودکار اعمال یا ذخیره نمی‌شود. در صورت نیاز، آن را کپی و دستی ویرایش کنید.
      </p>
    </section>
  );
}
