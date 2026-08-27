import Link from "next/link";
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

const ICON_SRC: Record<QuickAccessItem["tone"], string> = {
  news: "/home/icon-news.png",
  tools: "/home/icon-tools.png",
  prompt: "/home/icon-prompt.png",
  roadmap: "/home/icon-roadmap.png",
  learning: "/home/icon-learning.png",
};

const ICON_GLOW: Record<QuickAccessItem["tone"], string> = {
  news: "drop-shadow-[0_10px_24px_rgba(148,163,184,0.35)]",
  tools: "drop-shadow-[0_10px_24px_rgba(124,58,237,0.45)]",
  prompt: "drop-shadow-[0_10px_24px_rgba(59,130,246,0.45)]",
  roadmap: "drop-shadow-[0_10px_24px_rgba(124,58,237,0.42)]",
  learning: "drop-shadow-[0_10px_24px_rgba(168,85,247,0.45)]",
};

/**
 * Premium SaaS navigation cards — Linear / Vercel / Raycast density.
 */
export function QuickAccessSection() {
  return (
    <section
      className="home-quick-access relative pt-6 pb-5 sm:pt-7 sm:pb-6 lg:pt-8 lg:pb-7"
      aria-labelledby="quick-access-heading"
    >
      <PublicContainer size="wide">
        <h2
          id="quick-access-heading"
          className="mb-7 flex items-center justify-start gap-2.5 text-[24px] font-extrabold tracking-tight text-white sm:mb-8 sm:text-[28px]"
        >
          <span
            className="inline-flex h-7 w-7 shrink-0 items-center justify-center text-[#A855F7] drop-shadow-[0_0_14px_rgba(168,85,247,0.65)]"
            aria-hidden
          >
            <ZapIcon />
          </span>
          دسترسی سریع
        </h2>

        <ul className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-5">
          {QUICK_ACCESS_ITEMS.map((item) => (
            <li key={item.id} className="min-w-0">
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
      className="group focus-ring relative flex min-h-[240px] w-full flex-col items-center rounded-[22px] border border-white/[0.08] px-4 pb-5 pt-5 no-underline transition duration-300 hover:-translate-y-2 hover:border-[rgba(124,58,237,0.55)] hover:shadow-[0_0_35px_rgba(124,58,237,0.25)]"
      style={{
        backgroundImage: "linear-gradient(145deg, #111827, #080d1c)",
      }}
    >
      <span
        className="pointer-events-none absolute inset-0 overflow-hidden rounded-[22px] opacity-0 transition duration-300 group-hover:opacity-100"
        style={{
          background:
            "radial-gradient(ellipse 80% 55% at 50% 0%, rgba(124,58,237,0.16), transparent 70%)",
        }}
        aria-hidden
      />

      <span
        className={[
          "relative mx-auto flex h-16 w-16 shrink-0 items-center justify-center transition duration-300 group-hover:scale-110 sm:h-[72px] sm:w-[72px]",
          ICON_GLOW[item.tone],
        ].join(" ")}
        aria-hidden
      >
        <img
          src={ICON_SRC[item.tone]}
          alt=""
          width={72}
          height={72}
          decoding="async"
          className="h-full w-full object-contain"
        />
      </span>

      <h3 className="relative mt-3 text-center text-[16px] font-bold leading-snug text-white sm:text-[18px]">
        {item.title}
      </h3>
      <p className="relative mt-1.5 line-clamp-2 min-h-[2.5rem] flex-1 px-1 text-center text-[12px] leading-5 text-[#94A3B8] sm:text-[13px]">
        {item.description}
      </p>

      <span
        className="relative z-10 mt-3 flex h-6 w-6 shrink-0 items-center justify-center text-[#A855F7] transition duration-300 group-hover:-translate-x-1"
        aria-hidden
      >
        <ArrowIcon />
      </span>
    </Link>
  );
}

function ArrowIcon() {
  return (
    <svg
      width="22"
      height="22"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.8"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden
    >
      <path d="M19 12H5" />
      <path d="M12 19l-7-7 7-7" />
    </svg>
  );
}

function ZapIcon() {
  return (
    <svg width="22" height="22" viewBox="0 0 24 24" fill="currentColor" aria-hidden>
      <path d="M13 2 4.5 13.2c-.35.46-.02 1.1.55 1.1H11l-1 7.8 8.7-11.4c.34-.45.01-1.1-.55-1.1H13l0-7.6Z" />
    </svg>
  );
}
