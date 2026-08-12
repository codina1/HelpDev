"use client";

import { ContentAiPanel } from "@/components/admin/content/ai/content-ai-panel";
import { ContentDetailTabs } from "@/components/admin/content/details/content-detail-tabs";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { useContentAiAssistant } from "@/lib/admin/content/content-hooks";

export function ContentAiWorkspace({ contentId }: { contentId: string }) {
  const { status, result, activeAction, error, run } = useContentAiAssistant(contentId);

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="دستیار هوش مصنوعی"
        description="پیشنهادهای تحریریه‌ای با تأیید انسانی — بدون ذخیرهٔ خودکار"
      />
      <ContentDetailTabs id={contentId} active="ai" />
      <ContentAiPanel
        status={status}
        result={result}
        activeAction={activeAction}
        error={error}
        onRun={run}
      />
    </div>
  );
}
