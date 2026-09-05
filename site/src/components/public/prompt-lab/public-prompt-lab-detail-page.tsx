import { ApiClientError } from "@/lib/api/errors";
import type { PromptLabCardItem } from "@/lib/public/prompt-lab-mock";
import type { PromptLabDetail } from "@/lib/public/prompt-lab-detail-mock";
import { PUBLIC_PROMPT_LAB_PATH } from "@/lib/public/prompt-lab-routes";
import { buildPromptDetailViewModel } from "@/data/prompt-detail";
import { PromptDetailView } from "@/components/prompt/PromptDetailView";
import { ArticlesContainer } from "@/components/articles/articles-container";
import { Button } from "@/components/ui/ds/button";

type PublicPromptLabDetailPageProps = {
  detail: PromptLabDetail;
  related: readonly PromptLabCardItem[];
  similar: readonly PromptLabCardItem[];
};

/**
 * Public Prompt Lab detail — GET /api/v1/prompts/{slug}.
 * Premium marketplace-style detail layout.
 */
export function PublicPromptLabDetailPage({
  detail,
  related,
  similar,
}: PublicPromptLabDetailPageProps) {
  const model = buildPromptDetailViewModel({
    detail,
    similar: similar.length > 0 ? similar : related,
  });

  return <PromptDetailView model={model} />;
}

export function PublicPromptLabDetailError({ error }: { error?: unknown }) {
  const message =
    error instanceof ApiClientError && error.isNetworkError
      ? "اتصال به سرور برقرار نشد. اتصال اینترنت را بررسی کنید."
      : "بارگذاری این پرامپت ناموفق بود. کمی بعد دوباره تلاش کنید.";

  return (
    <div className="bg-[#050816] py-16" dir="rtl">
      <ArticlesContainer>
        <div
          className="rounded-2xl border border-white/[0.08] bg-[#0B1224] px-6 py-10 text-center shadow-[0_0_40px_rgba(139,92,246,0.12)]"
          role="alert"
        >
          <h1 className="text-[22px] font-extrabold text-white">پرامپت در دسترس نیست</h1>
          <p className="mt-3 text-[14px] leading-7 text-[#94A3B8]">{message}</p>
          <div className="mt-6 flex justify-center">
            <Button href={PUBLIC_PROMPT_LAB_PATH} size="sm" variant="secondary">
              بازگشت به Prompt Lab
            </Button>
          </div>
        </div>
      </ArticlesContainer>
    </div>
  );
}
