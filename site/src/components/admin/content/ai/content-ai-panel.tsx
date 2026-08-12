"use client";

import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { AiActionCard } from "@/components/admin/content/ai/ai-action-card";
import { AiLoadingState } from "@/components/admin/content/ai/ai-loading-state";
import { AiResultViewer } from "@/components/admin/content/ai/ai-result-viewer";
import type { ContentAiActionKey } from "@/lib/admin/content/content-hooks";
import type { ContentAiResult, ContentAiStatus } from "@/lib/admin/content/content-types";

const ACTIONS: Array<{
  key: ContentAiActionKey;
  title: string;
  description: string;
}> = [
  {
    key: "analyze",
    title: "تحلیل محتوا",
    description: "پیشنهاد تحریریه‌ای درباره ساختار و وضوح — بدون امتیاز یا رتبه‌بندی.",
  },
  {
    key: "title-suggestions",
    title: "پیشنهاد عنوان",
    description: "چند عنوان پیشنهادی بر اساس نسخهٔ ذخیره‌شده.",
  },
  {
    key: "meta-description",
    title: "ساخت توضیحات SEO",
    description: "یک توضیح متا پیشنهادی (بدون ادعای حجم کلیدواژه).",
  },
  {
    key: "faq",
    title: "ساخت FAQ",
    description: "چند پرسش و پاسخ متداول مبتنی بر محتوا.",
  },
  {
    key: "outline",
    title: "ساخت ساختار مقاله",
    description: "پیشنهاد ساختار سرفصل‌ها برای مقاله.",
  },
];

type ContentAiPanelProps = {
  status: ContentAiStatus;
  result: ContentAiResult | null;
  activeAction: ContentAiActionKey | null;
  error?: unknown;
  onRun: (action: ContentAiActionKey) => void;
};

/**
 * Content Studio — AI Assistant tab.
 * Human approval only: show result → copy/apply manually. No auto-replace.
 */
export function ContentAiPanel({
  status,
  result,
  activeAction,
  error,
  onRun,
}: ContentAiPanelProps) {
  const isLoading = status === "loading";

  return (
    <section className="space-y-4" aria-labelledby="content-ai-heading">
      <div className="space-y-1">
        <h2 id="content-ai-heading" className="adm-text text-[15px] font-bold">
          دستیار هوش مصنوعی
        </h2>
        <p className="adm-subtle text-[12px] leading-6">
          پیشنهادها بر اساس آخرین نسخهٔ ذخیره‌شده تولید می‌شوند و هرگز به‌صورت خودکار در محتوا ذخیره یا
          منتشر نمی‌شوند. تأیید انسانی الزامی است.
        </p>
      </div>

      <div className="grid gap-3 sm:grid-cols-2">
        {ACTIONS.map((action) => (
          <AiActionCard
            key={action.key}
            title={action.title}
            description={action.description}
            busy={isLoading && activeAction === action.key}
            disabled={isLoading}
            onRun={() => onRun(action.key)}
          />
        ))}
      </div>

      {isLoading ? <AiLoadingState /> : null}

      {error ? (
        <AdminErrorState
          error={error}
          title="تولید پیشنهاد ناموفق بود"
          onRetry={activeAction ? () => onRun(activeAction) : undefined}
          showHome={false}
        />
      ) : null}

      {result && !isLoading ? <AiResultViewer result={result} /> : null}
    </section>
  );
}
