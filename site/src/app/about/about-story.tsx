import { PublicContainer } from "@/components/ui/public/v2/public-container";
import styles from "./about-story.module.css";

export const ABOUT_STORY_STEPS = [
  { id: "idea", title: "ایده اولیه" },
  { id: "platform", title: "ساخت پلتفرم" },
  { id: "tools", title: "توسعه ابزارها" },
  { id: "community", title: "جامعه مهندسان" },
] as const;

const NUMBER_FA = ["۱", "۲", "۳", "۴"] as const;

/**
 * About story — vertical RTL timeline, titles only.
 */
export function AboutStory() {
  return (
    <section className={`about-story ${styles.section}`} aria-labelledby="about-story-heading">
      <PublicContainer size="wide">
        <h2 id="about-story-heading" className={styles.heading}>
          داستان HelpDev
        </h2>
        <div className={styles.track}>
          <span className={styles.line} aria-hidden />
          <ol className={styles.list}>
          {ABOUT_STORY_STEPS.map((step, index) => (
            <li key={step.id} className={styles.item} style={{ animationDelay: `${0.08 + index * 0.12}s` }}>
              <span className={styles.node} aria-hidden>
                {NUMBER_FA[index]}
              </span>
              <article className={styles.card}>
                <h3 className={styles.title}>{step.title}</h3>
              </article>
            </li>
          ))}
          </ol>
        </div>
      </PublicContainer>
    </section>
  );
}
