import styles from "./team-member-card.module.css";

export type TeamMemberSocials = {
  github?: string;
  linkedin?: string;
  x?: string;
};

export type TeamMember = {
  id: string;
  name: string;
  role: string;
  bio: string;
  initials: string;
  avatarUrl?: string;
  socials?: TeamMemberSocials;
};

type TeamMemberCardProps = {
  member: TeamMember;
};

/** Reusable team card — avatar, name, role, bio, socials. */
export function TeamMemberCard({ member }: TeamMemberCardProps) {
  return (
    <article className={`team-member-card ${styles.card}`}>
      <div className={styles.avatar} aria-hidden>
        {member.avatarUrl ? (
          // eslint-disable-next-line @next/next/no-img-element
          <img src={member.avatarUrl} alt="" className={styles.photo} />
        ) : (
          <span className={styles.initials}>{member.initials}</span>
        )}
      </div>
      <h3 className={styles.name}>{member.name}</h3>
      <p className={styles.role}>{member.role}</p>
      <p className={styles.bio}>{member.bio}</p>
      {member.socials ? (
        <ul className={styles.socials} aria-label={`شبکه‌های ${member.name}`}>
          {member.socials.github ? (
            <li>
              <a className={styles.social} href={member.socials.github} aria-label="GitHub">
                <GitHubIcon />
              </a>
            </li>
          ) : null}
          {member.socials.linkedin ? (
            <li>
              <a className={styles.social} href={member.socials.linkedin} aria-label="LinkedIn">
                <LinkedInIcon />
              </a>
            </li>
          ) : null}
          {member.socials.x ? (
            <li>
              <a className={styles.social} href={member.socials.x} aria-label="X">
                <XIcon />
              </a>
            </li>
          ) : null}
        </ul>
      ) : null}
    </article>
  );
}

function GitHubIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="currentColor" aria-hidden>
      <path d="M12 2C6.48 2 2 6.58 2 12.26c0 4.52 2.87 8.35 6.84 9.71.5.1.68-.22.68-.49 0-.24-.01-.87-.01-1.71-2.78.62-3.37-1.37-3.37-1.37-.45-1.18-1.11-1.5-1.11-1.5-.91-.64.07-.63.07-.63 1 .07 1.53 1.06 1.53 1.06.9 1.57 2.36 1.12 2.94.86.09-.67.35-1.12.63-1.38-2.22-.26-4.56-1.14-4.56-5.07 0-1.12.39-2.03 1.03-2.75-.1-.26-.45-1.3.1-2.71 0 0 .84-.27 2.75 1.05a9.2 9.2 0 0 1 5 0c1.91-1.32 2.75-1.05 2.75-1.05.55 1.41.2 2.45.1 2.71.64.72 1.03 1.63 1.03 2.75 0 3.94-2.34 4.8-4.57 5.06.36.32.68.94.68 1.9 0 1.38-.01 2.49-.01 2.83 0 .27.18.6.69.49A10.04 10.04 0 0 0 22 12.26C22 6.58 17.52 2 12 2Z" />
    </svg>
  );
}

function LinkedInIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="currentColor" aria-hidden>
      <path d="M6.5 9H4V20h2.5V9ZM5.25 4A1.75 1.75 0 1 0 5.26 7.5 1.75 1.75 0 0 0 5.25 4ZM20 20h-2.5v-5.6c0-1.56-.56-2.62-1.96-2.62-1.07 0-1.7.72-1.98 1.41-.1.25-.12.6-.12.95V20H11V9h2.4v1.51c.4-.72 1.32-1.75 3.22-1.75 2.35 0 4.38 1.54 4.38 4.85V20Z" />
    </svg>
  );
}

function XIcon() {
  return (
    <svg width="13" height="13" viewBox="0 0 24 24" fill="currentColor" aria-hidden>
      <path d="M14.7 10.3 21.4 3h-1.9l-5.6 6.1L9.4 3H3.6l7.1 10.1L3.6 21h1.9l6-6.6L15.6 21h5.8L14.7 10.3ZM5.9 4.3h2.3l10 15.4h-2.3L5.9 4.3Z" />
    </svg>
  );
}
