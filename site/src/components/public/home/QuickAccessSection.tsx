import Link from "next/link";
import type { ReactNode } from "react";
import { PublicContainer } from "@/components/ui/public/v2/public-container";

export type QuickAccessItem = {
  id: string;
  title: string;
  description: string;
  href: string;
  tone: "news" | "tools" | "prompt" | "roadmap" | "learning";
};

export const QUICK_ACCESS_ITEMS: readonly QuickAccessItem[] = [
  {
    id: "news",
    title: "اخبار",
    description: "آخرین اخبار دنیای توسعه",
    href: "/news",
    tone: "news",
  },
  {
    id: "tools",
    title: "ابزارها",
    description: "ابزارهای کاربردی توسعه‌دهندگان",
    href: "/toolbox",
    tone: "tools",
  },
  {
    id: "prompt-lab",
    title: "Prompt Lab",
    description: "پرامپت‌های آماده هوش مصنوعی",
    href: "/prompt-lab",
    tone: "prompt",
  },
  {
    id: "roadmap",
    title: "Roadmap",
    description: "مسیرهای یادگیری",
    href: "/roadmap",
    tone: "roadmap",
  },
  {
    id: "learning",
    title: "یادگیری",
    description: "دوره‌ها و آموزش‌ها",
    href: "/learning",
    tone: "learning",
  },
] as const;

const ICON_GLOW: Record<QuickAccessItem["tone"], string> = {
  news: "shadow-[0_12px_32px_rgba(59,130,246,0.35),0_0_40px_rgba(148,163,184,0.18)]",
  tools: "shadow-[0_12px_32px_rgba(124,58,237,0.45),0_0_40px_rgba(168,85,247,0.28)]",
  prompt: "shadow-[0_12px_32px_rgba(6,182,212,0.42),0_0_40px_rgba(34,211,238,0.25)]",
  roadmap: "shadow-[0_12px_32px_rgba(124,58,237,0.42),0_0_40px_rgba(99,102,241,0.25)]",
  learning: "shadow-[0_12px_32px_rgba(37,99,235,0.42),0_0_40px_rgba(59,130,246,0.25)]",
};

/**
 * Premium SaaS navigation cards — Linear / Vercel / Raycast density.
 */
export function QuickAccessSection() {
  return (
    <section
      className="home-quick-access relative pt-20 pb-8 sm:pb-9 lg:pb-10"
      aria-labelledby="quick-access-heading"
    >
      <PublicContainer size="wide" className="max-w-[1280px]">
        <h2
          id="quick-access-heading"
          className="mb-7 text-start text-[24px] font-extrabold tracking-tight text-white sm:mb-8 sm:text-[28px]"
        >
          دسترسی سریع
        </h2>

        <ul className="grid grid-cols-2 gap-4 md:grid-cols-3 lg:grid-cols-5">
          {QUICK_ACCESS_ITEMS.map((item) => (
            <li key={item.id} className="flex min-w-0 justify-center">
              <QuickAccessCard item={item} />
            </li>
          ))}
        </ul>
      </PublicContainer>
    </section>
  );
}

function QuickAccessCard({ item }: { item: QuickAccessItem }) {
  return (
    <Link
      href={item.href}
      className="group focus-ring relative flex h-[180px] w-full max-w-[220px] flex-col overflow-hidden rounded-[22px] border border-white/[0.08] px-5 pb-[68px] pt-5 no-underline transition duration-300 hover:-translate-y-2 hover:border-[rgba(124,58,237,0.55)] hover:shadow-[0_0_35px_rgba(124,58,237,0.25)] lg:w-[220px]"
      style={{
        backgroundImage: "linear-gradient(145deg, #111827, #080d1c)",
      }}
    >
      <span
        className="pointer-events-none absolute inset-0 opacity-0 transition duration-300 group-hover:opacity-100"
        style={{
          background:
            "radial-gradient(ellipse 80% 55% at 50% 0%, rgba(124,58,237,0.16), transparent 70%)",
        }}
        aria-hidden
      />

      <span
        className={[
          "relative mx-auto flex h-16 w-16 shrink-0 items-center justify-center transition duration-300 group-hover:scale-110",
          ICON_GLOW[item.tone],
        ].join(" ")}
        aria-hidden
      >
        <Icon3D tone={item.tone} />
      </span>

      <h3 className="relative mt-3 text-start text-[18px] font-bold leading-snug text-white">
        {item.title}
      </h3>
      <p className="relative mt-1 line-clamp-2 text-start text-[13px] leading-5 text-[#94A3B8]">
        {item.description}
      </p>

      <span
        className="absolute bottom-5 left-1/2 flex h-6 w-6 -translate-x-1/2 items-center justify-center text-[#A855F7] transition duration-300 group-hover:translate-x-[-50%] group-hover:translate-y-1"
        aria-hidden
      >
        <ArrowIcon />
      </span>
    </Link>
  );
}

/** Layered gradient SVGs — read as 3D product icons. */
function Icon3D({ tone }: { tone: QuickAccessItem["tone"] }) {
  const id = `qa-${tone}`;

  if (tone === "news") {
    return (
      <svg width="64" height="64" viewBox="0 0 64 64" fill="none" aria-hidden>
        <defs>
          <linearGradient id={`${id}-a`} x1="12" y1="8" x2="52" y2="56" gradientUnits="userSpaceOnUse">
            <stop stopColor="#94A3B8" />
            <stop offset="0.55" stopColor="#3B82F6" />
            <stop offset="1" stopColor="#1E3A8A" />
          </linearGradient>
          <linearGradient id={`${id}-b`} x1="20" y1="16" x2="44" y2="48" gradientUnits="userSpaceOnUse">
            <stop stopColor="#E2E8F0" />
            <stop offset="1" stopColor="#60A5FA" />
          </linearGradient>
          <filter id={`${id}-s`} x="-20%" y="-20%" width="140%" height="140%">
            <feDropShadow dx="0" dy="6" stdDeviation="4" floodColor="#3B82F6" floodOpacity="0.45" />
          </filter>
        </defs>
        <g filter={`url(#${id}-s)`}>
          <rect x="10" y="12" width="36" height="42" rx="4" fill={`url(#${id}-a)`} />
          <rect x="18" y="10" width="36" height="42" rx="4" fill={`url(#${id}-b)`} opacity="0.95" />
          <rect x="24" y="18" width="22" height="3.5" rx="1.5" fill="#1E293B" opacity="0.55" />
          <rect x="24" y="26" width="18" height="2.5" rx="1" fill="#1E293B" opacity="0.4" />
          <rect x="24" y="32" width="20" height="2.5" rx="1" fill="#1E293B" opacity="0.35" />
          <rect x="24" y="40" width="14" height="8" rx="2" fill="#2563EB" opacity="0.55" />
        </g>
      </svg>
    );
  }

  if (tone === "tools") {
    return (
      <svg width="64" height="64" viewBox="0 0 64 64" fill="none" aria-hidden>
        <defs>
          <linearGradient id={`${id}-a`} x1="8" y1="8" x2="56" y2="56" gradientUnits="userSpaceOnUse">
            <stop stopColor="#E9D5FF" />
            <stop offset="0.45" stopColor="#A855F7" />
            <stop offset="1" stopColor="#5B21B6" />
          </linearGradient>
          <linearGradient id={`${id}-b`} x1="28" y1="6" x2="52" y2="40" gradientUnits="userSpaceOnUse">
            <stop stopColor="#C4B5FD" />
            <stop offset="1" stopColor="#7C3AED" />
          </linearGradient>
          <filter id={`${id}-s`} x="-20%" y="-20%" width="140%" height="140%">
            <feDropShadow dx="0" dy="6" stdDeviation="4" floodColor="#7C3AED" floodOpacity="0.5" />
          </filter>
        </defs>
        <g filter={`url(#${id}-s)`}>
          <path
            d="M38 10c-5.5 0-10 4.5-10 10 0 1.4.3 2.7.8 3.9L14 38.7 18.3 43l14.8-14.8c1.2.5 2.5.8 3.9.8 5.5 0 10-4.5 10-10 0-1.2-.2-2.3-.6-3.4L40 22l-4-4 6.4-6.4c-1.1-.4-2.2-.6-3.4-.6Z"
            fill={`url(#${id}-a)`}
          />
          <path d="M42 14l8 8-4.5 1.5-5-5L42 14Z" fill={`url(#${id}-b)`} />
          <circle cx="20" cy="44" r="5" fill="#A855F7" opacity="0.85" />
          <circle cx="20" cy="44" r="2.2" fill="#F5F3FF" opacity="0.7" />
        </g>
      </svg>
    );
  }

  if (tone === "prompt") {
    return (
      <svg width="64" height="64" viewBox="0 0 64 64" fill="none" aria-hidden>
        <defs>
          <linearGradient id={`${id}-a`} x1="10" y1="8" x2="54" y2="52" gradientUnits="userSpaceOnUse">
            <stop stopColor="#A5F3FC" />
            <stop offset="0.5" stopColor="#06B6D4" />
            <stop offset="1" stopColor="#0E7490" />
          </linearGradient>
          <linearGradient id={`${id}-b`} x1="22" y1="18" x2="42" y2="36" gradientUnits="userSpaceOnUse">
            <stop stopColor="#ECFEFF" />
            <stop offset="1" stopColor="#67E8F9" />
          </linearGradient>
          <filter id={`${id}-s`} x="-20%" y="-20%" width="140%" height="140%">
            <feDropShadow dx="0" dy="6" stdDeviation="4" floodColor="#06B6D4" floodOpacity="0.5" />
          </filter>
        </defs>
        <g filter={`url(#${id}-s)`}>
          <path
            d="M12 14h40a4 4 0 0 1 4 4v22a4 4 0 0 1-4 4H28l-10 8v-8H12a4 4 0 0 1-4-4V18a4 4 0 0 1 4-4Z"
            fill={`url(#${id}-a)`}
          />
          <circle cx="24" cy="28" r="3" fill={`url(#${id}-b)`} />
          <circle cx="32" cy="28" r="3" fill={`url(#${id}-b)`} opacity="0.9" />
          <circle cx="40" cy="28" r="3" fill={`url(#${id}-b)`} opacity="0.8" />
          <path d="M46 12l2.2 4.4L53 19l-4.4 2.2L46 26l-2.2-4.8L39 19l4.8-2.6L46 12Z" fill="#ECFEFF" opacity="0.9" />
        </g>
      </svg>
    );
  }

  if (tone === "roadmap") {
    return (
      <svg width="64" height="64" viewBox="0 0 64 64" fill="none" aria-hidden>
        <defs>
          <linearGradient id={`${id}-a`} x1="8" y1="10" x2="56" y2="54" gradientUnits="userSpaceOnUse">
            <stop stopColor="#DDD6FE" />
            <stop offset="0.5" stopColor="#8B5CF6" />
            <stop offset="1" stopColor="#4C1D95" />
          </linearGradient>
          <linearGradient id={`${id}-b`} x1="20" y1="16" x2="48" y2="48" gradientUnits="userSpaceOnUse">
            <stop stopColor="#C4B5FD" />
            <stop offset="1" stopColor="#7C3AED" />
          </linearGradient>
          <filter id={`${id}-s`} x="-20%" y="-20%" width="140%" height="140%">
            <feDropShadow dx="0" dy="6" stdDeviation="4" floodColor="#7C3AED" floodOpacity="0.5" />
          </filter>
        </defs>
        <g filter={`url(#${id}-s)`}>
          <path
            d="M14 48V18l12 4 12-6 12 4v28l-12-4-12 6-12-4Z"
            fill={`url(#${id}-a)`}
          />
          <path d="M26 22v28M38 16v28" stroke={`url(#${id}-b)`} strokeWidth="2.5" strokeLinecap="round" />
          <circle cx="26" cy="34" r="3.5" fill="#F5F3FF" />
          <circle cx="38" cy="30" r="3.5" fill="#EDE9FE" />
          <circle cx="48" cy="24" r="2.5" fill="#C4B5FD" />
        </g>
      </svg>
    );
  }

  return (
    <svg width="64" height="64" viewBox="0 0 64 64" fill="none" aria-hidden>
      <defs>
        <linearGradient id={`${id}-a`} x1="10" y1="10" x2="54" y2="54" gradientUnits="userSpaceOnUse">
          <stop stopColor="#BFDBFE" />
          <stop offset="0.45" stopColor="#3B82F6" />
          <stop offset="1" stopColor="#1E3A8A" />
        </linearGradient>
        <linearGradient id={`${id}-b`} x1="18" y1="14" x2="46" y2="40" gradientUnits="userSpaceOnUse">
          <stop stopColor="#DBEAFE" />
          <stop offset="1" stopColor="#2563EB" />
        </linearGradient>
        <filter id={`${id}-s`} x="-20%" y="-20%" width="140%" height="140%">
          <feDropShadow dx="0" dy="6" stdDeviation="4" floodColor="#2563EB" floodOpacity="0.5" />
        </filter>
      </defs>
      <g filter={`url(#${id}-s)`}>
        <path d="M12 24 32 14l20 10-20 10L12 24Z" fill={`url(#${id}-a)`} />
        <path d="M18 28v14c0 2 6 6 14 6s14-4 14-6V28" fill={`url(#${id}-b)`} opacity="0.92" />
        <path d="M32 34v14" stroke="#EFF6FF" strokeWidth="2" strokeLinecap="round" opacity="0.55" />
        <rect x="28" y="20" width="8" height="3" rx="1" fill="#EFF6FF" opacity="0.7" />
      </g>
    </svg>
  );
}

function ArrowIcon(): ReactNode {
  return (
    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" aria-hidden>
      <path d="M12 5v14M12 19l-5-5M12 19l5-5" />
    </svg>
  );
}
