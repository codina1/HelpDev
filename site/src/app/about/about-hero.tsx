import { PublicContainer } from "@/components/ui/public/v2/public-container";
import styles from "./about-hero.module.css";

const TITLE_LEAD = "ما در HelpDev";
const TITLE_MAIN = "آینده مهندسی نرم‌افزار را می‌سازیم";
export const ABOUT_HERO_TITLE = `${TITLE_LEAD} ${TITLE_MAIN}`;
export const ABOUT_HERO_SUBTITLE =
  "پلتفرمی برای مهندسانی که می‌خواهند با ترکیب دانش مهندسی و هوش مصنوعی بهتر بسازند.";

const PARTICLES = [
  { x: 12, y: 22, d: "0s" },
  { x: 78, y: 16, d: "0.7s" },
  { x: 22, y: 68, d: "1.2s" },
  { x: 88, y: 58, d: "1.8s" },
  { x: 46, y: 12, d: "0.4s" },
  { x: 64, y: 80, d: "2.1s" },
  { x: 8, y: 44, d: "1.5s" },
  { x: 92, y: 34, d: "0.9s" },
  { x: 38, y: 88, d: "2.6s" },
  { x: 70, y: 8, d: "1.1s" },
] as const;

const NODES = [
  { id: "knowledge", label: "Knowledge", x: 24, y: 22 },
  { id: "ai", label: "AI", x: 78, y: 26 },
  { id: "systems", label: "Systems", x: 22, y: 76 },
  { id: "build", label: "Build", x: 80, y: 74 },
] as const;

/**
 * About page hero only — no extra sections.
 */
export function AboutHero() {
  return (
    <section className={styles.hero} aria-labelledby="about-hero-title">
      <div className={styles.grid} aria-hidden />
      <div className={styles.glowPurple} aria-hidden />
      <div className={styles.glowCyan} aria-hidden />
      <div className={styles.glowBlue} aria-hidden />
      {PARTICLES.map((particle) => (
        <span
          key={`${particle.x}-${particle.y}`}
          className={styles.particle}
          style={{ left: `${particle.x}%`, top: `${particle.y}%`, ["--d" as string]: particle.d }}
          aria-hidden
        />
      ))}

      <PublicContainer size="wide" className={styles.inner}>
        <div className={styles.copy}>
          <p className={styles.eyebrow}>AI Engineering Platform</p>
          <h1 id="about-hero-title" className={styles.title}>
            <span className={styles.titleAccent}>{TITLE_LEAD}</span>
            <span className={styles.titlePlain}>{TITLE_MAIN}</span>
          </h1>
          <p className={styles.subtitle}>{ABOUT_HERO_SUBTITLE}</p>
        </div>

        <div
          className={`${styles.visual} ${styles.float}`}
          role="img"
          aria-label="هسته هوش مهندسی HelpDev AI"
        >
          <div className={styles.orbGlow} aria-hidden />
          <span className={`${styles.ring} ${styles.ringA}`} aria-hidden />
          <span className={`${styles.ring} ${styles.ringB}`} aria-hidden />
          <span className={styles.scanner} aria-hidden />
          <svg className="absolute inset-0 h-full w-full" viewBox="0 0 100 100" aria-hidden>
            <defs>
              <linearGradient id="about-hero-edge" x1="0%" y1="0%" x2="100%" y2="100%">
                <stop offset="0%" stopColor="var(--home-purple)" />
                <stop offset="100%" stopColor="var(--home-cyan)" />
              </linearGradient>
            </defs>
            {NODES.map((node) => (
              <line
                key={node.id}
                x1="50"
                y1="50"
                x2={node.x}
                y2={node.y}
                stroke="url(#about-hero-edge)"
                strokeWidth="0.5"
                opacity="0.75"
              />
            ))}
          </svg>
          <div className={styles.core}>
            <span className={styles.coreOrb}>AI</span>
            <span className={styles.coreLabel}>HelpDev AI</span>
          </div>
          {NODES.map((node) => (
            <span
              key={node.id}
              className={styles.node}
              style={{ left: `${node.x}%`, top: `${node.y}%` }}
            >
              <span className={styles.chip}>{node.label.slice(0, 2)}</span>
              <span className={styles.label}>{node.label}</span>
            </span>
          ))}
        </div>
      </PublicContainer>
    </section>
  );
}
