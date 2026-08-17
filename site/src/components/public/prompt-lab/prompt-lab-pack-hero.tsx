import Link from "next/link";
import { PublicContainer } from "@/components/ui/public/v2/public-container";
import type { PromptLabPack } from "@/lib/public/prompt-lab-pack-mock";
import { PUBLIC_PROMPT_LAB_PATH } from "@/lib/public/prompt-lab-routes";
import styles from "./prompt-lab-pack-hero.module.css";

const NUMBER_FA = new Intl.NumberFormat("fa-IR");

type PromptLabPackHeroProps = {
  pack: PromptLabPack;
};

export function PromptLabPackHero({ pack }: PromptLabPackHeroProps) {
  return (
    <header className={styles.hero} aria-labelledby="prompt-lab-pack-title">
      <div className={styles.grid} aria-hidden />
      <div className={styles.glowPurple} aria-hidden />
      <div className={styles.glowCyan} aria-hidden />
      <PublicContainer size="wide" className={styles.inner}>
        <div className={styles.coverWrap}>
          <img src={pack.coverImage} alt="" className={styles.cover} />
          <span className={styles.coverShade} aria-hidden />
        </div>
        <div className={styles.meta}>
          <nav className={styles.crumb} aria-label="مسیر صفحه">
            <Link href={PUBLIC_PROMPT_LAB_PATH} className={styles.crumbLink}>
              Prompt Lab
            </Link>
            <span aria-hidden>/</span>
            <span>پک پرامپت</span>
          </nav>
          <h1 id="prompt-lab-pack-title" className={styles.title}>
            {pack.title}
          </h1>
          <p className={styles.description}>{pack.description}</p>
          <div className={styles.badges}>
            <span className={`${styles.badge} ${styles.category}`}>{pack.category}</span>
            <span className={`${styles.badge} ${styles.count}`}>
              {NUMBER_FA.format(pack.items.length)} پرامپت
            </span>
          </div>
        </div>
      </PublicContainer>
    </header>
  );
}
