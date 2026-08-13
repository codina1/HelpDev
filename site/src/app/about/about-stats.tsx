import styles from "./about-stats.module.css";

const NUMBER_FA = new Intl.NumberFormat("fa-IR");

function formatStatValue(value: number): string {
  if (!Number.isFinite(value) || value <= 0) return "۰";
  return `+${NUMBER_FA.format(value)}`;
}

export type AboutStatItem = {
  id: "engineers" | "articles" | "tools" | "paths";
  label: string;
  value: number;
};

export const ABOUT_STAT_LABELS = {
  engineers: "مهندس فعال",
  articles: "مقاله تخصصی",
  tools: "ابزار معرفی شده",
  paths: "مسیر یادگیری",
} as const;

type AboutStatsProps = {
  counts: {
    articles: number;
    tools: number;
    paths: number;
  };
};

/**
 * About stats — compact glass bar. Values are catalog-derived; missing stay ۰.
 */
export function AboutStats({ counts }: AboutStatsProps) {
  const items: AboutStatItem[] = [
    { id: "engineers", label: ABOUT_STAT_LABELS.engineers, value: 0 },
    { id: "articles", label: ABOUT_STAT_LABELS.articles, value: counts.articles },
    { id: "tools", label: ABOUT_STAT_LABELS.tools, value: counts.tools },
    { id: "paths", label: ABOUT_STAT_LABELS.paths, value: counts.paths },
  ];

  return (
    <section className={`about-stats ${styles.section}`} aria-label="آمار پلتفرم">
      <div className={styles.row}>
        {items.map((item, index) => (
          <div key={item.id} className={`${styles.stat} ${index > 0 ? styles.split : ""}`}>
            <span className={styles.icon} aria-hidden>
              <StatIcon id={item.id} />
            </span>
            <div className={styles.copy}>
              <p className={styles.value}>{formatStatValue(item.value)}</p>
              <p className={styles.label}>{item.label}</p>
            </div>
          </div>
        ))}
      </div>
    </section>
  );
}

function StatIcon({ id }: { id: AboutStatItem["id"] }) {
  const common = {
    width: 14,
    height: 14,
    viewBox: "0 0 24 24",
    fill: "none",
    stroke: "currentColor",
    strokeWidth: 1.75,
  } as const;

  if (id === "engineers") {
    return (
      <svg {...common}>
        <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" />
        <circle cx="9" cy="7" r="3" />
        <path d="M22 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75" />
      </svg>
    );
  }
  if (id === "articles") {
    return (
      <svg {...common}>
        <path d="M7 3h8l5 5v13H7z" />
        <path d="M15 3v5h5M10 13h7M10 17h5" />
      </svg>
    );
  }
  if (id === "tools") {
    return (
      <svg {...common}>
        <path d="M14.7 6.3a4 4 0 0 0-5.4 5.4L3 18l3 3 6.3-6.3a4 4 0 0 0 5.4-5.4l-3 3-2-2 3-3Z" />
      </svg>
    );
  }
  return (
    <svg {...common}>
      <circle cx="6" cy="6" r="2.2" />
      <circle cx="18" cy="12" r="2.2" />
      <circle cx="8" cy="18" r="2.2" />
      <path d="M8 7.5 16 11M16.5 14 9.5 17" />
    </svg>
  );
}
