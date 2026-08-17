import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { PromptLabContentViewer } from "@/components/public/prompt-lab/prompt-lab-content-viewer";
import { PromptLabDetailAuthor } from "@/components/public/prompt-lab/prompt-lab-detail-author";
import { PromptLabDetailHero } from "@/components/public/prompt-lab/prompt-lab-detail-hero";
import { PromptLabDetailSidebar } from "@/components/public/prompt-lab/prompt-lab-detail-sidebar";
import { PromptLabPromptsSection } from "@/components/public/prompt-lab/prompt-lab-prompts-section";
import type { PromptLabCardItem } from "@/lib/public/prompt-lab-mock";
import type { PromptLabDetail } from "@/lib/public/prompt-lab-detail-mock";
import styles from "./public-prompt-lab-detail-page.module.css";

type PublicPromptLabDetailPageProps = {
  detail: PromptLabDetail;
  related: readonly PromptLabCardItem[];
  similar: readonly PromptLabCardItem[];
};

/**
 * Public Prompt Lab detail — local mock catalog, no API.
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
