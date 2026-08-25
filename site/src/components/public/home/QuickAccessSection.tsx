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

const ICON_SHELL: Record<QuickAccessItem["tone"], string> = {
  news: "from-[#64748B]/50 via-[#3B82F6]/35 to-[#1E293B]/40 text-[#93C5FD] shadow-[0_0_28px_rgba(59,130,246,0.35)]",
  tools: "from-[#7C3AED]/55 via-[#A855F7]/30 to-[#4C1D95]/40 text-[#D8B4FE] shadow-[0_0_28px_rgba(124,58,237,0.4)]",
  prompt: "from-[#06B6D4]/50 via-[#22D3EE]/28 to-[#0E7490]/35 text-[#67E8F9] shadow-[0_0_28px_rgba(6,182,212,0.4)]",
  roadmap: "from-[#7C3AED]/55 via-[#6366F1]/30 to-[#4C1D95]/40 text-[#C4B5FD] shadow-[0_0_28px_rgba(124,58,237,0.4)]",
  learning: "from-[#2563EB]/55 via-[#3B82F6]/30 to-[#1E3A8A]/40 text-[#93C5FD] shadow-[0_0_28px_rgba(37,99,235,0.4)]",
};

/**
 * Premium SaaS quick-access feature cards (Linear / Vercel / Raycast).
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
          className="mb-6 text-start text-[1.25rem] font-extrabold text-white sm:mb-7 sm:text-[1.35rem]"
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
      className="group focus-ring flex h-[180px] w-full max-w-[220px] flex-col rounded-[20px] border border-white/[0.08] bg-[#0B1224] p-6 no-underline transition duration-300 hover:-translate-y-[6px] hover:border-[rgba(124,58,237,0.55)] hover:shadow-[0_0_30px_rgba(124,58,237,0.25)] lg:w-[220px]"
    >
      <span
        className={[
          "mx-auto flex h-14 w-14 shrink-0 items-center justify-center rounded-2xl bg-gradient-to-br transition duration-300 group-hover:scale-110",
          ICON_SHELL[item.tone],
        ].join(" ")}
        aria-hidden
      >
        <QuickAccessIcon tone={item.tone} />
      </span>

      <h3 className="mt-3 text-start text-[18px] font-bold leading-snug text-white">{item.title}</h3>
      <p className="mt-1.5 line-clamp-2 flex-1 text-start text-[13px] leading-5 text-[#94A3B8]">
        {item.description}
      </p>

      <span className="mt-auto inline-flex text-[#A855F7] transition duration-300 group-hover:translate-x-[-2px]" aria-hidden>
        <ArrowIcon />
      </span>
    </Link>
  );
}

function QuickAccessIcon({ tone }: { tone: QuickAccessItem["tone"] }) {
  const common = {
    width: 28,
    height: 28,
    viewBox: "0 0 24 24",
    fill: "none",
    stroke: "currentColor",
    strokeWidth: 1.55,
  } as const;

  if (tone === "news") {
    return (
      <svg {...common} aria-hidden>
        <path d="M4 5h12a2 2 0 0 1 2 2v12H6a2 2 0 0 1-2-2V5Z" />
        <path d="M18 7h2a2 2 0 0 1 2 2v8a3 3 0 0 1-3 3" />
        <path d="M8 10h6M8 14h4" />
      </svg>
    );
  }
  if (tone === "tools") {
    return (
      <svg {...common} aria-hidden>
        <path d="M14.7 6.3a4 4 0 0 0-5.4 5.4L3 18l3 3 6.3-6.3a4 4 0 0 0 5.4-5.4l-3 3-2-2 3-3Z" />
      </svg>
    );
  }
  if (tone === "prompt") {
    return (
      <svg {...common} aria-hidden>
        <path d="M5 6h14v9H9l-4 3V6Z" />
        <path d="M9 10h6M9 13h4" />
      </svg>
    );
  }
  if (tone === "roadmap") {
    return (
      <svg {...common} aria-hidden>
        <path d="M4 19V7l6 2 6-3 4 2v12l-4-2-6 3-6-2Z" />
        <path d="M10 9v12M16 6v12" />
      </svg>
    );
  }
  return (
    <svg {...common} aria-hidden>
      <path d="M3 9 12 5l9 4-9 4-9-4Z" />
      <path d="M7 11.5v5.2c0 .6 2.2 2.3 5 2.3s5-1.7 5-2.3v-5.2" />
    </svg>
  );
}

function ArrowIcon(): ReactNode {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" aria-hidden>
      <path d="M15 6 9 12l6 6" />
    </svg>
  );
}
