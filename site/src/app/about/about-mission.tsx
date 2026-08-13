import { PublicContainer } from "@/components/ui/public/v2/public-container";
import styles from "./about-mission.module.css";

export const ABOUT_MISSION_ITEMS = [
  {
    id: "knowledge",
    title: "دانش مهندسی",
    description: "انتقال تجربه‌های واقعی توسعه نرم‌افزار",
    accent: "purple",
  },
  {
    id: "ai",
    title: "هوش مصنوعی",
    description: "استفاده کاربردی از AI در فرآیند توسعه",
    accent: "cyan",
  },
  {
    id: "path",
    title: "مسیر رشد",
    description: "کمک به مهندسان برای انتخاب مسیر درست",
    accent: "blue",
  },
] as const;

/**
 * About mission — three glass cards only.
 */
export function AboutMission() {
  return (
    <section className={`about-mission ${styles.section}`} aria-labelledby="about-mission-heading">
      <PublicContainer size="wide">
        <h2 id="about-mission-heading" className={styles.heading}>
          ماموریت
        </h2>
        <ul className={styles.grid}>
          {ABOUT_MISSION_ITEMS.map((item) => (
            <li key={item.id} className={`${styles.card} ${styles[item.accent]}`}>
              <span className={styles.icon} aria-hidden>
                <MissionIcon id={item.id} />
              </span>
              <h3 className={styles.title}>{item.title}</h3>
              <p className={styles.copy}>{item.description}</p>
            </li>
          ))}
        </ul>
      </PublicContainer>
    </section>
  );
}

function MissionIcon({ id }: { id: (typeof ABOUT_MISSION_ITEMS)[number]["id"] }) {
  const common = {
    width: 18,
    height: 18,
    viewBox: "0 0 24 24",
    fill: "none",
    stroke: "currentColor",
    strokeWidth: 1.7,
  } as const;

  if (id === "knowledge") {
    return (
      <svg {...common}>
        <path d="M7 3h8l5 5v13H7z" />
        <path d="M15 3v5h5M10 13h7M10 17h5" />
      </svg>
    );
  }
  if (id === "ai") {
    return (
      <svg {...common}>
        <path d="M12 3l1.5 5.5L19 10l-5.5 1.5L12 17l-1.5-5.5L5 10l5.5-1.5L12 3z" />
      </svg>
    );
  }
  return (
    <svg {...common}>
      <circle cx="6" cy="6" r="2.1" />
      <circle cx="18" cy="12" r="2.1" />
      <circle cx="8" cy="18" r="2.1" />
      <path d="M8 7.4 16.2 11M16.4 14 9.6 17" />
    </svg>
  );
}
