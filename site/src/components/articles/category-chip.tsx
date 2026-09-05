"use client";

import { useEffect, useRef } from "react";
import { ARTICLE_CATEGORY_CHIPS, type ArticleCategoryId } from "@/data/articles";

type CategoryChipBarProps = {
  active: ArticleCategoryId;
  onSelect: (value: ArticleCategoryId) => void;
};

const ICON_COLOR: Record<string, string> = {
  all: "#FFFFFF",
  ai: "#A78BFA",
  programming: "#C084FC",
  dotnet: "#818CF8",
  frontend: "#60A5FA",
  backend: "#34D399",
  devops: "#38BDF8",
  tools: "#A5B4FC",
  architecture: "#A78BFA",
  security: "#F87171",
};

function CategoryIcon({ name, color }: { name: string; color: string }) {
  const cls = "h-4 w-4 shrink-0";
  switch (name) {
    case "ai":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <circle cx="12" cy="12" r="3.2" stroke={color} strokeWidth="1.6" />
          <path d="M12 3.5v3M12 17.5v3M3.5 12h3M17.5 12h3M6.2 6.2l2.1 2.1M15.7 15.7l2.1 2.1M17.8 6.2l-2.1 2.1M8.3 15.7l-2.1 2.1" stroke={color} strokeWidth="1.6" strokeLinecap="round" />
        </svg>
      );
    case "programming":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M8.2 8.5 4.5 12l3.7 3.5M15.8 8.5 19.5 12l-3.7 3.5M13.2 6.5l-2.4 11" stroke={color} strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
      );
    case "dotnet":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <circle cx="12" cy="12" r="8" stroke={color} strokeWidth="1.6" />
          <path d="M8 12h8M12 8v8" stroke={color} strokeWidth="1.6" strokeLinecap="round" />
        </svg>
      );
    case "frontend":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="3.2" y="4.2" width="17.6" height="12.8" rx="2" stroke={color} strokeWidth="1.55" />
          <path d="M3.2 8.4h17.6M8.5 20h7M12 17v3" stroke={color} strokeWidth="1.55" strokeLinecap="round" />
        </svg>
      );
    case "backend":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="3.8" y="4.2" width="16.4" height="6" rx="1.6" stroke={color} strokeWidth="1.55" />
          <rect x="3.8" y="13.8" width="16.4" height="6" rx="1.6" stroke={color} strokeWidth="1.55" />
          <circle cx="7.4" cy="7.2" r="0.9" fill={color} />
          <circle cx="7.4" cy="16.8" r="0.9" fill={color} />
        </svg>
      );
    case "devops":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M8.4 8.6c1.8-2.3 5-2.3 6.8 0 1.4 1.8 1.4 4.2 0 6-1.8 2.3-5 2.3-6.8 0-1.4-1.8-1.4-4.2 0-6Z" stroke={color} strokeWidth="1.6" strokeLinejoin="round" />
        </svg>
      );
    case "tools":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M14.5 5.5a3.2 3.2 0 0 0 4 4l-4 4a3.2 3.2 0 0 0 0-4ZM9.5 18.5l-4-4 2.2-2.2 4 4-2.2 2.2Z" stroke={color} strokeWidth="1.55" strokeLinejoin="round" />
        </svg>
      );
    case "architecture":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M4.5 18.5h15M7 18.5V9.5l5-4 5 4v9" stroke={color} strokeWidth="1.55" strokeLinejoin="round" />
          <path d="M10 18.5v-4h4v4" stroke={color} strokeWidth="1.55" strokeLinejoin="round" />
        </svg>
      );
    case "security":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M12 3.5 5.5 6.2v5.2c0 4.2 2.8 7.5 6.5 8.8 3.7-1.3 6.5-4.6 6.5-8.8V6.2L12 3.5Z" stroke={color} strokeWidth="1.6" strokeLinejoin="round" />
        </svg>
      );
    default:
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="3.5" y="3.5" width="7" height="7" rx="1.5" stroke={color} strokeWidth="1.6" />
          <rect x="13.5" y="3.5" width="7" height="7" rx="1.5" stroke={color} strokeWidth="1.6" />
          <rect x="3.5" y="13.5" width="7" height="7" rx="1.5" stroke={color} strokeWidth="1.6" />
          <rect x="13.5" y="13.5" width="7" height="7" rx="1.5" stroke={color} strokeWidth="1.6" />
        </svg>
      );
  }
}

/** Compact category pills — one row on desktop · scroll only on small screens. */
export function ArticleCategoryChipBar({ active, onSelect }: CategoryChipBarProps) {
  const scrollerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const node = scrollerRef.current;
    if (!node) return;

    function onWheel(event: WheelEvent) {
      if (!node || event.deltaY === 0) return;
      if (node.scrollWidth <= node.clientWidth + 2) return;
      if (Math.abs(event.deltaY) <= Math.abs(event.deltaX)) return;
      event.preventDefault();
      node.scrollLeft += event.deltaY;
    }

    node.addEventListener("wheel", onWheel, { passive: false });
    return () => node.removeEventListener("wheel", onWheel);
  }, []);

  return (
    <div
      id="articles-categories"
      ref={scrollerRef}
      dir="rtl"
      className="min-w-0 w-full overflow-x-auto overscroll-x-contain pb-1 [scrollbar-width:none] [&::-webkit-scrollbar]:hidden lg:overflow-x-visible"
    >
      <div
        className="flex w-max min-w-full flex-nowrap items-center gap-2 lg:w-full lg:justify-between lg:gap-2"
        role="toolbar"
        aria-label="دسته‌بندی مقالات"
      >
        {ARTICLE_CATEGORY_CHIPS.map((item) => {
          const isActive = active === item.id;
          return (
            <button
              key={item.id}
              type="button"
              aria-pressed={isActive}
              onClick={() => onSelect(item.id)}
              className={[
                "inline-flex h-10 shrink-0 items-center gap-1.5 rounded-2xl border px-3.5 text-[12.5px] font-semibold transition duration-200 lg:flex-1 lg:justify-center lg:px-2 xl:px-3.5",
                isActive
                  ? "border-transparent bg-gradient-to-l from-[#7C3AED] via-[#6D28D9] to-[#4F46E5] text-white shadow-[0_0_20px_rgba(124,58,237,0.5)]"
                  : "border-white/10 bg-[#0F1626]/90 text-[#E5E7EB] backdrop-blur-sm hover:border-[rgba(168,85,247,0.4)] hover:text-white",
              ].join(" ")}
            >
              <span className="whitespace-nowrap">{item.label}</span>
              <CategoryIcon
                name={item.icon}
                color={isActive ? "#FFFFFF" : ICON_COLOR[item.icon] ?? "#A78BFA"}
              />
            </button>
          );
        })}
      </div>
    </div>
  );
}
