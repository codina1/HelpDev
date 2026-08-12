"use client";

import Link from "next/link";

const FOOTER_LINKS = [
  { href: "/about", label: "درباره ما" },
  { href: "/contact", label: "تماس با ما" },
  { href: "/privacy", label: "حریم خصوصی" },
  { href: "/terms", label: "شرایط استفاده" },
] as const;

export function HomeFooter() {
  return (
    <footer className="mt-4 space-y-0 overflow-hidden rounded-2xl border border-white/[0.07] bg-[#0d101c] shadow-[inset_0_1px_0_rgba(255,255,255,0.04)]">
      {/* Newsletter bar */}
      <div className="border-b border-white/[0.06] px-5 py-6 sm:px-6 lg:px-8 lg:py-7">
        <div className="flex flex-col items-stretch gap-6 lg:flex-row lg:items-center lg:justify-between lg:gap-8">
          {/* فرم — سمت راست در RTL */}
          <form
            className="order-2 flex w-full shrink-0 gap-2 lg:order-1 lg:max-w-[340px]"
            onSubmit={(e) => e.preventDefault()}
          >
            <input
              type="email"
              placeholder="ایمیل خود را وارد کنید"
              className="focus-ring h-11 min-w-0 flex-1 rounded-xl border border-white/[0.08] bg-[#080a12] px-4 text-[13px] text-white outline-none transition-all placeholder:text-slate-500 focus:border-violet-500/40 focus:shadow-[0_0_0_3px_rgba(139,92,246,0.12)]"
              aria-label="ایمیل عضویت در خبرنامه"
            />
            <button
              type="submit"
              className="focus-ring shrink-0 rounded-xl bg-gradient-to-l from-violet-600 to-indigo-600 px-5 text-[13px] font-bold text-white shadow-[0_4px_20px_rgba(124,58,237,0.35)] transition-all duration-300 hover:-translate-y-0.5 hover:shadow-[0_6px_28px_rgba(124,58,237,0.45)] active:translate-y-0"
            >
              عضویت
            </button>
          </form>

          {/* متن مرکزی */}
          <div className="order-1 flex flex-1 flex-col items-center gap-2 text-center lg:order-2 lg:px-4">
            <div className="flex items-center gap-3">
              <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-amber-500/15 text-amber-400 ring-1 ring-amber-500/25">
                <MailIcon />
              </span>
              <div className="text-start">
                <p className="text-[14px] font-extrabold leading-6 text-white">
                  از جدیدترین مطالب و اخبار باخبر شوید
                </p>
                <p className="mt-0.5 text-[12px] leading-5 text-slate-400">
                  ایمیل خود را وارد کنید تا هیچ مطلب مهمی را از دست ندهید
                </p>
              </div>
            </div>
          </div>

          {/* شبکه‌های اجتماعی — سمت چپ در RTL */}
          <div className="order-3 flex items-center justify-center gap-2.5 lg:order-3 lg:justify-end">
            <SocialLink href="https://github.com" label="GitHub" icon="github" />
            <SocialLink href="https://t.me" label="Telegram" icon="telegram" />
            <SocialLink href="https://twitter.com" label="Twitter" icon="twitter" />
          </div>
        </div>
      </div>

      {/* Copyright + links */}
      <div className="flex flex-col-reverse items-center justify-between gap-4 px-5 py-4 sm:flex-row sm:px-6 lg:px-8">
        <p className="text-center text-[11px] text-slate-500 sm:text-end">
          © ۲۰۲۶ HelpDev. تمامی حقوق محفوظ است.
        </p>

        <nav className="flex flex-wrap items-center justify-center gap-x-5 gap-y-2 sm:justify-start">
          {FOOTER_LINKS.map((link) => (
            <Link
              key={link.href}
              href={link.href}
              className="focus-ring relative text-[12px] text-slate-400 transition-colors duration-200 hover:text-violet-300 after:absolute after:-bottom-0.5 after:start-0 after:h-px after:w-0 after:bg-violet-400 after:transition-all hover:after:w-full"
            >
              {link.label}
            </Link>
          ))}
        </nav>
      </div>
    </footer>
  );
}

function SocialLink({
  href,
  label,
  icon,
}: {
  href: string;
  label: string;
  icon: "github" | "telegram" | "twitter";
}) {
  return (
    <a
      href={href}
      target="_blank"
      rel="noopener noreferrer"
      aria-label={label}
      className="focus-ring flex h-10 w-10 items-center justify-center rounded-full border border-cyan-500/25 bg-cyan-500/[0.08] text-cyan-400 transition-all duration-300 hover:-translate-y-0.5 hover:border-cyan-400/45 hover:bg-cyan-500/15 hover:text-cyan-300 hover:shadow-[0_0_20px_rgba(34,211,238,0.15)]"
    >
      <SocialIcon type={icon} />
    </a>
  );
}

function MailIcon() {
  return (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden>
      <rect x="2" y="4" width="20" height="16" rx="2" />
      <path d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7" />
    </svg>
  );
}

function SocialIcon({ type }: { type: "github" | "telegram" | "twitter" }) {
  if (type === "github") {
    return (
      <svg width="17" height="17" viewBox="0 0 24 24" fill="currentColor" aria-hidden>
        <path d="M12 2C6.477 2 2 6.484 2 12.021c0 4.428 2.865 8.184 6.839 9.504.5.092.682-.217.682-.483 0-.237-.008-.868-.013-1.703-2.782.605-3.369-1.343-3.369-1.343-.454-1.158-1.11-1.466-1.11-1.466-.908-.62.069-.608.069-.608 1.003.07 1.531 1.032 1.531 1.032.892 1.53 2.341 1.088 2.91.832.092-.647.35-1.088.636-1.338-2.22-.253-4.555-1.113-4.555-4.951 0-1.093.39-1.988 1.029-2.688-.103-.253-.446-1.272.098-2.65 0 0 .84-.27 2.75 1.026A9.564 9.564 0 0 1 12 6.844c.85.004 1.705.115 2.504.337 1.909-1.296 2.747-1.027 2.747-1.027.546 1.379.202 2.398.1 2.651.64.7 1.028 1.595 1.028 2.688 0 3.848-2.339 4.695-4.566 4.943.359.309.678.92.678 1.855 0 1.338-.012 2.419-.012 2.747 0 .268.18.58.688.482A10.019 10.019 0 0 0 22 12.021C22 6.484 17.522 2 12 2z" />
      </svg>
    );
  }

  if (type === "telegram") {
    return (
      <svg width="17" height="17" viewBox="0 0 24 24" fill="currentColor" aria-hidden>
        <path d="M11.944 0A12 12 0 0 0 0 12a12 12 0 0 0 12 12 12 12 0 0 0 12-12A12 12 0 0 0 12 0a12 12 0 0 0-.056 0zm4.962 7.224c.1-.002.321.023.465.14a.506.506 0 0 1 .171.325c.016.093.036.306.02.472-.18 1.898-.962 6.502-1.36 8.627-.168.9-.499 1.201-.82 1.23-.696.065-1.225-.46-1.9-.902-1.056-.693-1.653-1.124-2.678-1.8-1.185-.78-.417-1.21.258-1.91.177-.184 3.247-2.977 3.307-3.23.007-.032.014-.15-.056-.212s-.174-.041-.249-.024c-.106.024-1.793 1.14-5.061 3.345-.48.33-.913.49-1.302.48-.428-.008-1.252-.241-1.865-.44-.752-.245-1.349-.374-1.297-.789.027-.216.325-.437.893-.663 3.498-1.524 5.83-2.529 6.998-3.014 3.332-1.386 4.025-1.627 4.476-1.635z" />
      </svg>
    );
  }

  return (
    <svg width="17" height="17" viewBox="0 0 24 24" fill="currentColor" aria-hidden>
      <path d="M18.244 2.25h3.308l-7.227 8.26 8.502 11.24H16.17l-5.214-6.817L4.99 21.75H1.68l7.73-8.835L1.254 2.25H8.08l4.713 6.231zm-1.161 17.52h1.833L7.084 4.126H5.117z" />
    </svg>
  );
}
