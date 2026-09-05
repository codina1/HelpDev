import Link from "next/link";
import { SITE } from "@/lib/constants";
import { PublicContainer } from "@/components/ui/public/v2/public-container";

/** Replace these assets to update footer brand / social marks. */
export const FOOTER_ICON_SLOTS = {
  brand: "/home/icon-brand.png",
  github: "/home/icon-social-github.png",
  telegram: "/home/icon-social-telegram.png",
  x: "/home/icon-social-x.png",
} as const;

const PRODUCT_LINKS = [
  { href: "/", label: "خانه" },
  { href: "/articles", label: "مقالات" },
  { href: "/roadmap", label: "نقشه راه" },
  { href: "/prompt-lab", label: "Prompt Lab" },
] as const;

const RESOURCE_LINKS = [
  { href: "/courses", label: "یادگیری" },
  { href: "/toolbox", label: "ابزارها" },
  { href: "/news", label: "اخبار" },
  { href: "/search", label: "جستجو" },
] as const;

const COMPANY_LINKS = [
  { href: "/about", label: "درباره ما" },
  { href: "/contact", label: "تماس" },
  { href: "/write", label: "نویسنده شو" },
  { href: "/starter-kit", label: "Starter Kit" },
] as const;

const COMMUNITY_LINKS = [
  { href: "https://github.com/codina1/HelpDev", label: "GitHub", external: true },
  { href: "/contact", label: "پشتیبانی" },
  { href: "/privacy", label: "حریم خصوصی" },
  { href: "/terms", label: "شرایط استفاده" },
] as const;

const SOCIAL_LINKS = [
  {
    href: "https://github.com/codina1/HelpDev",
    label: "GitHub",
    iconSrc: FOOTER_ICON_SLOTS.github,
    slot: "github",
  },
  {
    href: "/contact",
    label: "تماس",
    iconSrc: FOOTER_ICON_SLOTS.telegram,
    slot: "telegram",
  },
] as const;

/**
 * Site Footer — Design Reference columns, brand slot, social icon slots.
 */
export function Footer() {
  const year = new Date().getFullYear().toLocaleString("fa-IR", { useGrouping: false });

  return (
    <footer className="home-footer border-t border-white/[0.08] bg-[#050816]" dir="rtl">
      <PublicContainer size="wide" className="py-7 sm:py-8">
        <div className="grid gap-7 lg:grid-cols-[1.2fr_repeat(4,minmax(0,1fr))] lg:gap-6">
          <div className="max-w-sm text-start">
            <Link href="/" className="focus-ring inline-flex items-center gap-2.5 no-underline">
              <span className="flex h-8 w-8 items-center justify-center" aria-hidden>
                <img
                  src={FOOTER_ICON_SLOTS.brand}
                  alt=""
                  width={32}
                  height={32}
                  className="h-8 w-8 object-contain"
                  data-icon-slot="brand"
                />
              </span>
              <span className="text-[15px] font-semibold tracking-tight text-white">{SITE.name}</span>
            </Link>
            <p className="mt-2.5 text-[13px] leading-6 text-[#94A3B8]">{SITE.description}</p>
            <div className="mt-4 flex items-center gap-2">
              {SOCIAL_LINKS.map((social) => {
                const className =
                  "focus-ring inline-flex h-9 w-9 items-center justify-center rounded-[10px] border border-white/[0.08] bg-[#0B1224] transition hover:border-[rgba(124,58,237,0.45)] hover:shadow-[0_0_18px_rgba(124,58,237,0.25)]";
                const icon = (
                  <img
                    src={social.iconSrc}
                    alt=""
                    width={16}
                    height={16}
                    className="h-4 w-4 object-contain opacity-90"
                    data-icon-slot={social.slot}
                  />
                );
                if (social.href.startsWith("http")) {
                  return (
                    <a
                      key={social.slot}
                      href={social.href}
                      target="_blank"
                      rel="noreferrer"
                      className={className}
                      aria-label={social.label}
                    >
                      {icon}
                    </a>
                  );
                }
                return (
                  <Link key={social.slot} href={social.href} className={className} aria-label={social.label}>
                    {icon}
                  </Link>
                );
              })}
            </div>
          </div>

          <FooterColumn title="محصول" links={PRODUCT_LINKS} />
          <FooterColumn title="منابع" links={RESOURCE_LINKS} />
          <FooterColumn title="شرکت" links={COMPANY_LINKS} />
          <FooterColumn title="جامعه" links={COMMUNITY_LINKS} />
        </div>

        <div className="mt-6 flex flex-col gap-3 border-t border-white/[0.08] pt-5 sm:flex-row sm:items-center sm:justify-between">
          <p className="text-[12px] text-[#64748B]">
            © {year} {SITE.name}. تمامی حقوق محفوظ است.
          </p>
          <nav className="flex flex-wrap gap-4" aria-label="پیوندهای حقوقی">
            <Link href="/privacy" className="focus-ring text-[12px] text-[#94A3B8] no-underline hover:text-white">
              حریم خصوصی
            </Link>
            <Link href="/terms" className="focus-ring text-[12px] text-[#94A3B8] no-underline hover:text-white">
              شرایط استفاده
            </Link>
          </nav>
        </div>
      </PublicContainer>
    </footer>
  );
}

function FooterColumn({
  title,
  links,
}: {
  title: string;
  links: readonly { href: string; label: string; external?: boolean }[];
}) {
  return (
    <div className="text-start">
      <p className="text-[12px] font-bold tracking-wide text-white">{title}</p>
      <ul className="mt-3 space-y-2">
        {links.map((link) => (
          <li key={`${title}-${link.href}-${link.label}`}>
            {link.external || link.href.startsWith("http") ? (
              <a
                href={link.href}
                target="_blank"
                rel="noreferrer"
                className="focus-ring text-[13px] text-[#94A3B8] no-underline transition hover:text-white"
              >
                {link.label}
              </a>
            ) : (
              <Link
                href={link.href}
                className="focus-ring text-[13px] text-[#94A3B8] no-underline transition hover:text-white"
              >
                {link.label}
              </Link>
            )}
          </li>
        ))}
      </ul>
    </div>
  );
}
