import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { TeamMemberCard, type TeamMember } from "@/components/public/team/team-member-card";
import styles from "./about-team.module.css";

/** Placeholder roster only — not a published HelpDev staff list. */
export const ABOUT_TEAM_MOCK: readonly TeamMember[] = [
  {
    id: "sample-architect",
    name: "آرمین رضایی",
    role: "معمار نرم‌افزار",
    bio: "نمونه نمایشی — تمرکز روی طراحی سیستم و تصمیم‌های معماری.",
    initials: "آر",
    socials: { github: "#", linkedin: "#" },
  },
  {
    id: "sample-ai",
    name: "سارا محمدی",
    role: "مهندس AI",
    bio: "نمونه نمایشی — اتصال دانش منتشرشده به مسیر تحلیل و پاسخ.",
    initials: "سم",
    socials: { github: "#", x: "#" },
  },
  {
    id: "sample-frontend",
    name: "کیان احمدی",
    role: "مهندس فرانت‌اند",
    bio: "نمونه نمایشی — رابط محصول، تجربه RTL و سطح شیشه‌ای پلتفرم.",
    initials: "کا",
    socials: { github: "#", linkedin: "#", x: "#" },
  },
];

/**
 * About team — mock cards until a real roster exists.
 */
export function AboutTeam() {
  return (
    <section className={`about-team ${styles.section}`} aria-labelledby="about-team-heading">
      <PublicContainer size="wide">
        <h2 id="about-team-heading" className={styles.heading}>
          تیم
        </h2>
        <p className={styles.lead}>
          کارت‌های نمایشی عضو تیم. فهرست واقعی هنوز منتشر نشده و پایگاه داده ساخته نشده است.
        </p>
        <ul className={styles.grid}>
          {ABOUT_TEAM_MOCK.map((member) => (
            <li key={member.id}>
              <TeamMemberCard member={member} />
            </li>
          ))}
        </ul>
      </PublicContainer>
    </section>
  );
}
