import type { PromptLabAuthor } from "@/lib/public/prompt-lab-detail-mock";
import styles from "./prompt-lab-detail-author.module.css";

type PromptLabDetailAuthorProps = {
  author: PromptLabAuthor;
};

export function PromptLabDetailAuthor({ author }: PromptLabDetailAuthorProps) {
  return (
    <section className={styles.section} aria-labelledby="prompt-lab-author-heading">
      <h2 id="prompt-lab-author-heading" className={styles.heading}>
        درباره نویسنده
      </h2>
      <article className={styles.card}>
        <span className={styles.avatar} aria-hidden>
          {author.initials}
        </span>
        <div>
          <h3 className={styles.name}>{author.name}</h3>
          <p className={styles.role}>{author.role}</p>
          <p className={styles.bio}>{author.bio}</p>
        </div>
      </article>
    </section>
  );
}
