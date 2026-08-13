import { PublicContainer } from "@/components/ui/public/v2/public-container";
import styles from "./contact-info.module.css";

export const CONTACT_GITHUB = "https://github.com/codina1/HelpDev";

/**
 * Contact channels — GitHub is live; email / Telegram / LinkedIn are not published.
 */
export function ContactInfo() {
  return (
    <section className={`contact-info ${styles.section}`} aria-labelledby="contact-info-heading">
      <PublicContainer size="wide">
        <h2 id="contact-info-heading" className={styles.heading}>
          راه‌های ارتباط
        </h2>
        <ul className={styles.grid}>
          <li className={styles.card}>
            <span className={styles.icon} aria-hidden>
              <MailIcon />
            </span>
            <h3 className={styles.title}>ایمیل</h3>
            <p className={styles.copy}>
              ایمیل عمومی هنوز منتشر نشده. از فرم همین صفحه استفاده کنید.
            </p>
          </li>

          <li className={styles.card}>
            <span className={styles.icon} aria-hidden>
              <ShareIcon />
            </span>
            <h3 className={styles.title}>شبکه‌های اجتماعی</h3>
            <ul className={styles.socials}>
              <li>
                <span className={styles.muted}>
                  <TelegramIcon />
                  تلگرام — هنوز منتشر نشده
                </span>
              </li>
              <li>
                <span className={styles.muted}>
                  <LinkedInIcon />
                  لینکدین — هنوز منتشر نشده
                </span>
              </li>
              <li>
                <a
                  className={styles.link}
                  href={CONTACT_GITHUB}
                  target="_blank"
                  rel="noreferrer"
                >
                  <GitHubIcon />
                  GitHub
                </a>
              </li>
            </ul>
          </li>

          <li className={styles.card}>
            <span className={styles.icon} aria-hidden>
              <SupportIcon />
            </span>
            <h3 className={styles.title}>پشتیبانی</h3>
            <p className={styles.copy}>
              کانال پشتیبانی جداگانه فعال نیست. موضوع «گزارش مشکل» را در فرم انتخاب کنید.
            </p>
          </li>
        </ul>
      </PublicContainer>
    </section>
  );
}

function MailIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" aria-hidden>
      <rect x="3" y="5" width="18" height="14" rx="2" />
      <path d="m4 7 8 6 8-6" />
    </svg>
  );
}

function ShareIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" aria-hidden>
      <circle cx="18" cy="5" r="2.4" />
      <circle cx="6" cy="12" r="2.4" />
      <circle cx="18" cy="19" r="2.4" />
      <path d="m8.2 10.8 7.6-4.4M8.2 13.2l7.6 4.4" />
    </svg>
  );
}

function SupportIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" aria-hidden>
      <path d="M4 12a8 8 0 0 1 16 0v5a2 2 0 0 1-2 2h-2v-6h4" />
      <path d="M4 13h4v6H6a2 2 0 0 1-2-2v-4Z" />
    </svg>
  );
}

function TelegramIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" aria-hidden>
      <path d="M21 5 3 11.5l6.2 2.1L17 8l-6.2 7.2L11 21l2.2-4.2L21 5Z" />
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

function GitHubIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="currentColor" aria-hidden>
      <path d="M12 2C6.48 2 2 6.58 2 12.26c0 4.52 2.87 8.35 6.84 9.71.5.1.68-.22.68-.49 0-.24-.01-.87-.01-1.71-2.78.62-3.37-1.37-3.37-1.37-.45-1.18-1.11-1.5-1.11-1.5-.91-.64.07-.63.07-.63 1 .07 1.53 1.06 1.53 1.06.9 1.57 2.36 1.12 2.94.86.09-.67.35-1.12.63-1.38-2.22-.26-4.56-1.14-4.56-5.07 0-1.12.39-2.03 1.03-2.75-.1-.26-.45-1.3.1-2.71 0 0 .84-.27 2.75 1.05a9.2 9.2 0 0 1 5 0c1.91-1.32 2.75-1.05 2.75-1.05.55 1.41.2 2.45.1 2.71.64.72 1.03 1.63 1.03 2.75 0 3.94-2.34 4.8-4.57 5.06.36.32.68.94.68 1.9 0 1.38-.01 2.49-.01 2.83 0 .27.18.6.69.49A10.04 10.04 0 0 0 22 12.26C22 6.58 17.52 2 12 2Z" />
    </svg>
  );
}
