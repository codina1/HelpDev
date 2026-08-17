import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { PromptLabPackHero } from "@/components/public/prompt-lab/prompt-lab-pack-hero";
import { PromptLabPackTimeline } from "@/components/public/prompt-lab/prompt-lab-pack-timeline";
import type { PromptLabPack } from "@/lib/public/prompt-lab-pack-mock";
import styles from "./public-prompt-lab-pack-page.module.css";

type PublicPromptLabPackPageProps = {
  pack: PromptLabPack;
};

/**
 * Public Prompt Pack detail — local mock catalog, no API.
 */
export function PublicPromptLabPackPage({ pack }: PublicPromptLabPackPageProps) {
  return (
    <div className={styles.page} dir="rtl">
      <PromptLabPackHero pack={pack} />
      <PublicContainer size="wide" className={styles.body}>
        <PromptLabPackTimeline items={pack.items} />
      </PublicContainer>
    </div>
  );
}
