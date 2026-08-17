import { PublicContainer } from "@/components/ui/public/v2/public-container";
import type { PromptLabCardItem } from "@/lib/public/prompt-lab-mock";
import { PromptLabCard } from "./prompt-lab-card";
import styles from "./prompt-lab-prompts-section.module.css";

type PromptLabPromptsSectionProps = {
  id: string;
  headingId: string;
  title: string;
  lede: string;
  items: readonly PromptLabCardItem[];
};

export function PromptLabPromptsSection({
  id,
  headingId,
  title,
  lede,
  items,
}: PromptLabPromptsSectionProps) {
  return (
    <section id={id} className={styles.section} aria-labelledby={headingId}>
      <PublicContainer size="wide">
        <div className={styles.head}>
          <div>
            <h2 id={headingId} className={styles.heading}>
              {title}
            </h2>
            <p className={styles.lede}>{lede}</p>
          </div>
        </div>
        {items.length === 0 ? (
          <p className={styles.empty}>پرامپتی با این فیلتر پیدا نشد.</p>
        ) : (
          <ul className={styles.grid}>
            {items.map((item) => (
              <li key={item.id}>
                <PromptLabCard item={item} />
              </li>
            ))}
          </ul>
        )}
      </PublicContainer>
    </section>
  );
}
