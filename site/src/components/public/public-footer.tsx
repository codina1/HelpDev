import Link from "next/link";
import { SITE } from "@/lib/constants";

const FOOTER_COLUMNS = [
  {
    title: "Platform",
    links: [
      { href: "/articles", label: "Articles" },
      { href: "/roadmap", label: "Roadmaps" },
      { href: "/toolbox", label: "Tools" },
      { href: "/learning/assistant", label: "AI Assistant" },
    ],
  },
  {
    title: "Learning",
    links: [
      { href: "/courses", label: "Courses" },
      { href: "/articles", label: "Guides" },
      { href: "/learning", label: "Developer Paths" },
      { href: "/search", label: "Ask HelpDev AI" },
    ],
  },
  {
    title: "Community",
    links: [
      { href: "https://github.com", label: "GitHub", external: true },
      { href: "/write", label: "Newsletter" },
    ],
  },
  {
    title: "Company",
    links: [
      { href: "/", label: "About" },
      { href: "/settings", label: "Contact" },
    ],
  },
] as const;

/**
 * Premium footer — structured AI Engineering Knowledge Platform chrome.
 */
export function PublicFooter() {
  return (
    <footer className="mt-auto border-t border-[color:var(--pub-glass-border)] bg-[color:color-mix(in_srgb,var(--pub-bg)_92%,transparent)] backdrop-blur-xl">
      <div className="mx-auto grid max-w-6xl gap-8 px-4 py-10 sm:px-6 sm:grid-cols-2 lg:grid-cols-5 lg:py-14">
        <div className="lg:col-span-1">
          <p className="text-lg font-extrabold text-[color:var(--pub-fg)]">{SITE.name}</p>
          <p className="mt-2 max-w-xs text-sm leading-7 text-[color:var(--pub-muted)]">
            AI Engineering Knowledge Platform
          </p>
          <p className="mt-3 text-[12px] leading-6 text-[color:var(--pub-muted)]">
            سیستم عامل دانش مهندسی — از پرسش تا مسیر اجرا.
          </p>
        </div>
        {FOOTER_COLUMNS.map((col) => (
          <div key={col.title}>
            <p className="mb-3 text-[11px] font-bold tracking-wide text-[color:var(--pub-secondary)]">
              {col.title}
            </p>
            <ul className="space-y-2">
              {col.links.map((link) => (
                <li key={`${col.title}-${link.href}-${link.label}`}>
                  {"external" in link && link.external ? (
                    <a
                      href={link.href}
                      target="_blank"
                      rel="noreferrer"
                      className="focus-ring rounded text-sm text-[color:var(--pub-fg)]/85 transition hover:text-[color:var(--pub-primary)]"
                    >
                      {link.label}
                    </a>
                  ) : (
                    <Link
                      href={link.href}
                      className="focus-ring rounded text-sm text-[color:var(--pub-fg)]/85 transition hover:text-[color:var(--pub-primary)]"
                    >
                      {link.label}
                    </Link>
                  )}
                </li>
              ))}
            </ul>
          </div>
        ))}
      </div>
      <div className="border-t border-[color:var(--pub-glass-border)]">
        <div className="mx-auto flex max-w-6xl flex-col gap-2 px-4 py-4 text-[12px] text-[color:var(--pub-muted)] sm:flex-row sm:items-center sm:justify-between sm:px-6">
          <span>
            © {new Date().getFullYear()} {SITE.name}
          </span>
          <span>AI Engineering Operating System</span>
        </div>
      </div>
    </footer>
  );
}
