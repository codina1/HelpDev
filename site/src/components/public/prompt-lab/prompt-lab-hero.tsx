"use client";

import { Button } from "@/components/ui/ds/button";
import { Input } from "@/components/ui/ds/input";
import { PublicContainer } from "@/components/ui/public/v2/public-container";
import {
  PROMPT_LAB_HERO_SUBTITLE,
  PROMPT_LAB_HERO_TITLE,
} from "@/lib/public/prompt-lab-routes";
import styles from "./prompt-lab-hero.module.css";

export { PROMPT_LAB_HERO_SUBTITLE, PROMPT_LAB_HERO_TITLE };

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
  { id: "image", label: "Image", x: 24, y: 22 },
  { id: "code", label: "Code", x: 78, y: 26 },
  { id: "design", label: "Design", x: 22, y: 76 },
  { id: "write", label: "Write", x: 80, y: 74 },
] as const;

type PromptLabHeroProps = {
  query: string;
  onQueryChange: (value: string) => void;
  onSearch: () => void;
  onExplore: () => void;
};

/**
 * Prompt Lab public hero — title, subtitle, search, CTA, HelpDev AI visual.
 */
export function PromptLabHero({
  query,
  onQueryChange,
  onSearch,
  onExplore,
}: PromptLabHeroProps) {
  return (
    <section className={styles.hero} aria-labelledby="prompt-lab-hero-title">
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
          <p className={styles.eyebrow}>HelpDev AI Platform</p>
          <h1 id="prompt-lab-hero-title" className={styles.title}>
            {PROMPT_LAB_HERO_TITLE}
          </h1>
          <p className={styles.subtitle}>{PROMPT_LAB_HERO_SUBTITLE}</p>

          <div className={styles.actions}>
            <form
              className={styles.search}
              role="search"
              onSubmit={(event) => {
                event.preventDefault();
                onSearch();
              }}
            >
              <Input
                id="prompt-lab-search"
                type="search"
                value={query}
                onChange={onQueryChange}
                placeholder="جستجوی پرامپت، مدل یا دسته…"
                aria-label="جستجوی پرامپت"
                className={styles.searchInput}
              />
              <Button type="submit" size="md">
                جستجو
              </Button>
            </form>
            <div className={styles.ctaRow}>
              <Button type="button" size="lg" onClick={onExplore}>
                کاوش پرامپت‌ها
              </Button>
            </div>
          </div>
        </div>

        <div
          className={`${styles.visual} ${styles.float}`}
          role="img"
          aria-label="هسته Prompt Lab در هویت HelpDev AI"
        >
          <div className={styles.orbGlow} aria-hidden />
          <span className={`${styles.ring} ${styles.ringA}`} aria-hidden />
          <span className={`${styles.ring} ${styles.ringB}`} aria-hidden />
          <span className={styles.scanner} aria-hidden />
          <svg className="absolute inset-0 h-full w-full" viewBox="0 0 100 100" aria-hidden>
            <defs>
              <linearGradient id="prompt-lab-hero-edge" x1="0%" y1="0%" x2="100%" y2="100%">
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
                stroke="url(#prompt-lab-hero-edge)"
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
