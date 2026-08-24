import Link from "next/link";
import type { ReactNode } from "react";

export type HomeQuickAccessItem = {
  id: string;
  title: string;
  description: string;
  href: string;
  icon: "news" | "tools" | "prompt" | "roadmap" | "learning";
};

type HomeQuickAccessCardProps = {
  item: HomeQuickAccessItem;
};

/** Quick-access card — large icon, title, short copy, arrow CTA. */
export function HomeQuickAccessCard({ item }: HomeQuickAccessCardProps) {
  return (
    <li className="min-w-0">
      <Link
        href={item.href}
        className="group focus-ring relative flex h-full flex-col rounded-2xl border border-white/[0.08] bg-[#0B1224] p-5 no-underline transition duration-300 hover:-translate-y-1.5 hover:border-[rgba(124,58,237,0.45)] hover:shadow-[0_18px_48px_rgba(2,6,23,0.55),0_0_32px_rgba(124,58,237,0.28)] sm:p-6"
      >
        <span
          className="mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-gradient-to-br from-[rgba(124,58,237,0.28)] to-[rgba(6,182,212,0.12)] text-[#C4B5FD] shadow-[0_0_24px_rgba(124,58,237,0.2)] transition duration-300 group-hover:scale-105 group-hover:text-white group-hover:shadow-[0_0_32px_rgba(124,58,237,0.45)]"
          aria-hidden
        >
          <QuickAccessIcon name={item.icon} />
        </span>

        <h3 className="text-[15px] font-bold text-white sm:text-[16px]">{item.title}</h3>
        <p className="mt-2 flex-1 text-[12px] leading-6 text-[#94A3B8] sm:text-[13px]">
          {item.description}
        </p>

        <span
          className="mt-4 inline-flex h-8 w-8 items-center justify-center rounded-full border border-white/[0.08] bg-white/[0.03] text-[#94A3B8] transition duration-300 group-hover:border-[rgba(124,58,237,0.5)] group-hover:bg-[rgba(124,58,237,0.18)] group-hover:text-white"
          aria-hidden
        >
          <ArrowIcon />
        </span>
      </Link>
    </li>
  );
}

function QuickAccessIcon({ name }: { name: HomeQuickAccessItem["icon"] }) {
  const common = {
    width: 28,
    height: 28,
    viewBox: "0 0 24 24",
    fill: "none",
    stroke: "currentColor",
    strokeWidth: 1.6,
  } as const;

  if (name === "news") {
    return (
      <svg {...common} aria-hidden>
        <path d="M4 5h12a2 2 0 0 1 2 2v12H6a2 2 0 0 1-2-2V5Z" />
        <path d="M18 7h2a2 2 0 0 1 2 2v8a3 3 0 0 1-3 3" />
        <path d="M8 10h6M8 14h4" />
      </svg>
    );
  }
  if (name === "tools") {
    return (
      <svg {...common} aria-hidden>
        <path d="M14.7 6.3a4 4 0 0 0-5.4 5.4L3 18l3 3 6.3-6.3a4 4 0 0 0 5.4-5.4l-3 3-2-2 3-3Z" />
      </svg>
    );
  }
  if (name === "prompt") {
    return (
      <svg {...common} aria-hidden>
        <path d="M5 6h14v9H9l-4 3V6Z" />
        <path d="M9 10h6M9 13h4" />
      </svg>
    );
  }
  if (name === "roadmap") {
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
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden>
      <path d="M15 6 9 12l6 6" />
    </svg>
  );
}
