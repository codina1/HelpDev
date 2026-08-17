import Link from "next/link";
import { PublicContainer } from "@/components/ui/public/v2/public-container";
import type { PromptLabDetail } from "@/lib/public/prompt-lab-detail-mock";
import { PUBLIC_PROMPT_LAB_PATH } from "@/lib/public/prompt-lab-routes";
import styles from "./prompt-lab-detail-hero.module.css";

const NUMBER_FA = new Intl.NumberFormat("fa-IR");

type PromptLabDetailHeroProps = {
  detail: PromptLabDetail;
};

export function PromptLabDetailHero({ detail }: PromptLabDetailHeroProps) {
  return (
    <header className={styles.hero} aria-labelledby="prompt-lab-detail-title">
      <div className={styles.grid} aria-hidden />
      <div className={styles.glowPurple} aria-hidden />
      <div className={styles.glowCyan} aria-hidden />
      <PublicContainer size="wide" className={styles.inner}>
        <div className={styles.coverWrap}>
          <img src={detail.coverImage} alt="" className={styles.cover} />
          <span className={styles.coverShade} aria-hidden />
        </div>
        <div className={styles.meta}>
          <nav className={styles.crumb} aria-label="مسیر صفحه">
            <Link href={PUBLIC_PROMPT_LAB_PATH} className={styles.crumbLink}>
              Prompt Lab
            </Link>
            <span aria-hidden>/</span>
            <span>{detail.title}</span>
          </nav>
          <h1 id="prompt-lab-detail-title" className={styles.title}>
            {detail.title}
          </h1>
          <div className={styles.badges}>
            <span className={`${styles.badge} ${styles.category}`}>{detail.category}</span>
            <span className={`${styles.badge} ${styles.model}`}>{detail.aiModel}</span>
          </div>
          <p className={styles.author}>
            <span className={styles.avatar} aria-hidden>
              {detail.author.initials}
            </span>
            <span>
              <span className={styles.authorName}>{detail.author.name}</span>
              <span className={styles.authorRole}>{detail.author.role}</span>
            </span>
          </p>
          <dl className={styles.stats}>
            <div>
              <dt>بازدید</dt>
              <dd>{NUMBER_FA.format(detail.viewCount)}</dd>
            </div>
            <div>
              <dt>کپی</dt>
              <dd>{NUMBER_FA.format(detail.copyCount)}</dd>
            </div>
            <div>
              <dt>مدل</dt>
              <dd>{detail.aiModel}</dd>
            </div>
          </dl>
        </div>
      </PublicContainer>
    </header>
  );
}
