import Link from "next/link";
import type { PromptLabCardItem } from "@/lib/public/prompt-lab-mock";
import { publicPromptLabDetailPath } from "@/lib/public/prompt-lab-routes";
import styles from "./prompt-lab-card.module.css";

const NUMBER_FA = new Intl.NumberFormat("fa-IR");

type PromptLabCardProps = {
  item: PromptLabCardItem;
};

/** Glass prompt card — cover, title, description, model, category, copy/view counts. */
export function PromptLabCard({ item }: PromptLabCardProps) {
  return (
    <Link href={publicPromptLabDetailPath(item.slug)} className={styles.card}>
      <div className={styles.visual}>
        <img src={item.coverImage} alt="" className={styles.image} />
        <span className={styles.shade} aria-hidden />
      </div>
      <div className={styles.body}>
        <h3 className={styles.title}>{item.title}</h3>
        <p className={styles.description}>{item.description}</p>
        <div className={styles.badges}>
          <span className={`${styles.badge} ${styles.model}`}>{item.aiModel}</span>
          <span className={`${styles.badge} ${styles.category}`}>{item.category}</span>
        </div>
        <p className={styles.meta}>
          <span className={styles.metaItem}>
            <CopyIcon />
            {NUMBER_FA.format(item.copyCount)} کپی
          </span>
          <span className={styles.metaItem}>
            <ViewIcon />
            {NUMBER_FA.format(item.viewCount)} بازدید
          </span>
        </p>
      </div>
    </Link>
  );
}

function CopyIcon() {
  return (
    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden>
      <rect x="8" y="8" width="12" height="12" rx="2" />
      <path d="M4 16V6a2 2 0 0 1 2-2h10" />
    </svg>
  );
}

function ViewIcon() {
  return (
    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden>
      <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12z" />
      <circle cx="12" cy="12" r="2.5" />
    </svg>
  );
}
