import Link from "next/link";
import { SITE } from "@/lib/constants";
import { PublicFooterSubscribe } from "@/components/public/public-footer-subscribe";

const PRODUCT_LINKS = [
  { href: "/", label: "خانه" },
  { href: "/articles", label: "مقالات" },
  { href: "/roadmap", label: "نقشه راه" },
  { href: "/learning/assistant", label: "دستیار AI" },
] as const;

const LEARNING_LINKS = [
  { href: "/learning", label: "مسیر یادگیری" },
  { href: "/courses", label: "دوره‌ها" },
  { href: "/search", label: "جستجوی دانش" },
  { href: "/learning/assistant", label: "از AI بپرس" },
] as const;

const TOOL_LINKS = [
  { href: "/toolbox", label: "جعبه ابزار" },
  { href: "/prompt-lab", label: "Prompt Lab" },
  { href: "/starter-kit", label: "Starter Kit" },
  { href: "/cheat-sheets", label: "Cheat Sheets" },
] as const;

const COMPANY_LINKS = [
  { href: "/about", label: "درباره ما" },
  { href: "/contact", label: "تماس" },
  { href: "/news", label: "اخبار" },
  { href: "/write", label: "نویسنده شو" },
] as const;

const LEGAL_LINKS = [
  { href: "/privacy", label: "حریم خصوصی" },
  { href: "/terms", label: "شرایط استفاده" },
] as const;

const GITHUB_REPO = "https://github.com/codina1/HelpDev";

/**
 * Public footer — dark glass, RTL columns, real routes only.
 */
export function PublicFooter() {
  const year = new Date().getFullYear().toLocaleString("fa-IR", { useGrouping: false });

  return (
    <footer className="pub-footer" dir="rtl">
      <div className="pub-footer-inner">
        <div className="pub-footer-grid">
          <div className="pub-footer-brand">
            <Link href="/" className="pub-footer-logo focus-ring">
              <span className="pub-footer-mark" aria-hidden>
                H
              </span>
              <span className="pub-footer-wordmark">{SITE.name}</span>
            </Link>
            <p className="pub-footer-tagline">{SITE.description}</p>
            <Link href="/learning" className="pub-footer-access focus-ring">
              ورود به پلتفرم
            </Link>
            <div className="pub-footer-social">
              <a
                href={GITHUB_REPO}
                target="_blank"
                rel="noreferrer"
                className="pub-footer-social-link focus-ring"
                aria-label="مخزن GitHub هلپ‌دو"
              >
                <GitHubIcon />
              </a>
              <Link
                href="/contact"
                className="pub-footer-social-link focus-ring"
                aria-label="تماس"
              >
                <MailIcon />
              </Link>
            </div>
          </div>

          <FooterColumn title="محصول" links={PRODUCT_LINKS} />
          <FooterColumn title="یادگیری" links={LEARNING_LINKS} />
          <FooterColumn title="ابزارها" links={TOOL_LINKS} />
          <FooterColumn title="شرکت" links={COMPANY_LINKS} />
        </div>

        <div className="pub-footer-cta">
          <div className="pub-footer-cta-copy">
            <p className="pub-footer-cta-title">خبرنامه مهندسی</p>
            <p className="pub-footer-cta-lead">
              خلاصه مقالات و مسیرهای منتشرشده — وقتی سرویس خبرنامه آماده شود.
            </p>
          </div>
          <PublicFooterSubscribe />
        </div>

        <div className="pub-footer-bar">
          <p className="pub-footer-copy">
            © {year} {SITE.name}. تمامی حقوق محفوظ است.
          </p>
          <nav className="pub-footer-legal" aria-label="پیوندهای حقوقی">
            {LEGAL_LINKS.map((link) => (
              <Link key={link.href} href={link.href} className="pub-footer-legal-link focus-ring">
                {link.label}
              </Link>
            ))}
          </nav>
        </div>
      </div>
    </footer>
  );
}

function FooterColumn({
  title,
  links,
}: {
  title: string;
  links: readonly { href: string; label: string }[];
}) {
  return (
    <div className="pub-footer-col">
      <p className="pub-footer-col-title">{title}</p>
      <ul className="pub-footer-col-list">
        {links.map((link) => (
          <li key={`${title}-${link.href}-${link.label}`}>
            <Link href={link.href} className="pub-footer-link focus-ring">
              {link.label}
            </Link>
          </li>
        ))}
      </ul>
    </div>
  );
}

function GitHubIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor" aria-hidden>
      <path d="M12 2C6.477 2 2 6.484 2 12.021c0 4.428 2.865 8.184 6.839 9.504.5.092.682-.217.682-.483 0-.237-.008-.868-.013-1.703-2.782.605-3.369-1.343-3.369-1.343-.454-1.158-1.11-1.466-1.11-1.466-.908-.62.069-.608.069-.608 1.003.07 1.531 1.032 1.531 1.032.892 1.53 2.341 1.088 2.91.832.092-.647.35-1.088.636-1.338-2.22-.253-4.555-1.113-4.555-4.951 0-1.093.39-1.988 1.029-2.688-.103-.253-.446-1.272.098-2.65 0 0 .84-.27 2.75 1.026A9.564 9.564 0 0 1 12 6.844c.85.004 1.705.115 2.504.337 1.909-1.296 2.747-1.027 2.747-1.027.546 1.379.202 2.398.1 2.651.64.7 1.028 1.595 1.028 2.688 0 3.848-2.339 4.695-4.566 4.943.359.309.678.92.678 1.855 0 1.338-.012 2.419-.012 2.747 0 .268.18.58.688.482A10.019 10.019 0 0 0 22 12.021C22 6.484 17.522 2 12 2z" />
    </svg>
  );
}

function MailIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden>
      <rect x="3" y="5" width="18" height="14" rx="2" />
      <path d="m4 7 8 6 8-6" />
    </svg>
  );
}
