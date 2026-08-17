import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { Button } from "@/components/ui/ds/button";
import { PromptLabContentViewer } from "@/components/public/prompt-lab/prompt-lab-content-viewer";
import { PromptLabDetailAuthor } from "@/components/public/prompt-lab/prompt-lab-detail-author";
import { PromptLabDetailHero } from "@/components/public/prompt-lab/prompt-lab-detail-hero";
import { PromptLabDetailSidebar } from "@/components/public/prompt-lab/prompt-lab-detail-sidebar";
import { PromptLabPromptsSection } from "@/components/public/prompt-lab/prompt-lab-prompts-section";
import { ApiClientError } from "@/lib/api/errors";
import type { PromptLabCardItem } from "@/lib/public/prompt-lab-mock";
import type { PromptLabDetail } from "@/lib/public/prompt-lab-detail-mock";
import { PUBLIC_PROMPT_LAB_PATH } from "@/lib/public/prompt-lab-routes";
import styles from "./public-prompt-lab-detail-page.module.css";

type PublicPromptLabDetailPageProps = {
  detail: PromptLabDetail;
  related: readonly PromptLabCardItem[];
  similar: readonly PromptLabCardItem[];
};

/**
 * Public Prompt Lab detail — GET /api/v1/prompts/{slug}.
 */
export function PublicPromptLabDetailPage({
  detail,
  related,
  similar,
}: PublicPromptLabDetailPageProps) {
  return (
    <div className={styles.page} dir="rtl">
      <PromptLabDetailHero detail={detail} />
      <PublicContainer size="wide" className={styles.layout}>
        <div className={styles.main}>
          <section className={styles.description} aria-labelledby="prompt-lab-description-heading">
            <h2 id="prompt-lab-description-heading" className={styles.heading}>
              شرح پرامپت
            </h2>
            <p className={styles.lede}>{detail.description}</p>
          </section>
          <PromptLabContentViewer content={detail.content} />
        </div>
        <PromptLabDetailSidebar detail={detail} related={related} />
      </PublicContainer>
      <PromptLabPromptsSection
        id="prompt-lab-similar"
        headingId="prompt-lab-similar-heading"
        title="پرامپت‌های مشابه"
        lede="پرامپت‌هایی نزدیک به همین دسته، مدل یا برچسب."
        items={similar}
      />
      <PublicContainer size="wide" className={styles.bottom}>
        <PromptLabDetailAuthor author={detail.author} />
      </PublicContainer>
    </div>
  );
}

export function PublicPromptLabDetailError({ error }: { error?: unknown }) {
  const message =
    error instanceof ApiClientError && error.isNetworkError
      ? "اتصال به سرور برقرار نشد. اتصال اینترنت را بررسی کنید."
      : "بارگذاری این پرامپت ناموفق بود. کمی بعد دوباره تلاش کنید.";

  return (
    <div className={styles.page} dir="rtl">
      <PublicContainer size="wide">
        <div className={styles.error} role="alert">
          <h1 className={styles.errorTitle}>پرامپت در دسترس نیست</h1>
          <p className={styles.errorText}>{message}</p>
          <Button href={PUBLIC_PROMPT_LAB_PATH} size="sm" variant="secondary">
            بازگشت به Prompt Lab
          </Button>
        </div>
      </PublicContainer>
    </div>
  );
}
