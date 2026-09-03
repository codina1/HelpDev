"use client";

import { useEffect, useRef } from "react";
import {
  PROMPT_LAB_QUICK_FILTERS,
  type PromptLabQuickFilterId,
} from "@/data/prompt-lab";

type PromptLabCategoryBarProps = {
  active: PromptLabQuickFilterId;
  onSelect: (value: PromptLabQuickFilterId) => void;
};

const ICON_COLOR: Record<string, string> = {
  all: "#FFFFFF",
  chatgpt: "#34D399",
  claude: "#F59E0B",
  gemini: "#60A5FA",
  copilot: "#A5B4FC",
  code: "#C4B5FD",
  design: "#F472B6",
  devops: "#38BDF8",
  content: "#A78BFA",
  data: "#67E8F9",
  other: "#94A3B8",
};

function FilterIcon({ name, color }: { name: string; color: string }) {
  const cls = "h-5 w-5 shrink-0";
  switch (name) {
    case "chatgpt":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <circle cx="12" cy="12" r="8" stroke={color} strokeWidth="1.6" />
          <path d="M8.5 12h7M12 8.5v7" stroke={color} strokeWidth="1.6" strokeLinecap="round" />
        </svg>
      );
    case "claude":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M12 4 5.5 18h13L12 4Z" stroke={color} strokeWidth="1.6" strokeLinejoin="round" />
        </svg>
      );
    case "gemini":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M12 3v18M3 12h18M6.5 6.5l11 11M17.5 6.5l-11 11" stroke={color} strokeWidth="1.5" strokeLinecap="round" />
        </svg>
      );
    case "copilot":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <circle cx="9" cy="10" r="3.2" stroke={color} strokeWidth="1.6" />
          <circle cx="15.5" cy="10" r="2.4" stroke={color} strokeWidth="1.6" />
          <path d="M4.5 18c.7-2.6 2.8-4 4.5-4s3.8 1.4 4.5 4M14 14.2c1.4 0 3 .9 3.8 2.8" stroke={color} strokeWidth="1.6" strokeLinecap="round" />
        </svg>
      );
    case "code":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M9 7.2 4.2 12 9 16.8M15 7.2 19.8 12 15 16.8" stroke={color} strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
      );
    case "design":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <circle cx="12" cy="12" r="8" stroke={color} strokeWidth="1.6" />
          <circle cx="12" cy="12" r="2.4" fill={color} />
        </svg>
      );
    case "devops":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M8.4 8.6c1.8-2.3 5-2.3 6.8 0 1.4 1.8 1.4 4.2 0 6-1.8 2.3-5 2.3-6.8 0-1.4-1.8-1.4-4.2 0-6Z" stroke={color} strokeWidth="1.6" strokeLinejoin="round" />
        </svg>
      );
    case "content":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M6 4.5h9.2L18.5 8v11.5H6V4.5Z" stroke={color} strokeWidth="1.6" strokeLinejoin="round" />
          <path d="M15 4.6V8.2h3.6M9 12h6M9 15.2h4" stroke={color} strokeWidth="1.6" strokeLinecap="round" />
        </svg>
      );
    case "data":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <ellipse cx="12" cy="6.2" rx="7" ry="2.8" stroke={color} strokeWidth="1.55" />
          <path d="M5 6.2v11.4c0 1.55 3.1 2.8 7 2.8s7-1.25 7-2.8V6.2" stroke={color} strokeWidth="1.55" />
          <path d="M5 12c0 1.55 3.1 2.8 7 2.8s7-1.25 7-2.8" stroke={color} strokeWidth="1.55" />
        </svg>
      );
    case "other":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <circle cx="6" cy="12" r="1.5" fill={color} />
          <circle cx="12" cy="12" r="1.5" fill={color} />
          <circle cx="18" cy="12" r="1.5" fill={color} />
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

/** Horizontal quick-filter pills under the hero — always scrolls inside the container. */
export function PromptLabCategoryBar({ active, onSelect }: PromptLabCategoryBarProps) {
  const scrollerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const node = scrollerRef.current;
    if (!node) return;

    function onWheel(event: WheelEvent) {
      if (!node || event.deltaY === 0) return;
      if (Math.abs(event.deltaY) <= Math.abs(event.deltaX)) return;
      event.preventDefault();
      node.scrollLeft += event.deltaY;
    }

    node.addEventListener("wheel", onWheel, { passive: false });
    return () => node.removeEventListener("wheel", onWheel);
  }, []);

  return (
    <div
      ref={scrollerRef}
      dir="rtl"
      className="min-w-0 w-full overflow-x-auto overscroll-x-contain pb-2 [scrollbar-color:rgba(124,58,237,0.7)_rgba(15,22,38,0.9)] [scrollbar-width:thin] [&::-webkit-scrollbar]:h-1.5 [&::-webkit-scrollbar-thumb]:rounded-full [&::-webkit-scrollbar-thumb]:bg-[#7C3AED]/70 [&::-webkit-scrollbar-track]:bg-[#0F1626]"
    >
      <div className="flex w-max min-w-full flex-nowrap items-center gap-3" role="toolbar" aria-label="فیلتر سریع پرامپت‌ها">
        {PROMPT_LAB_QUICK_FILTERS.map((item) => {
          const isActive = active === item.id;
          return (
            <button
              key={item.id}
              type="button"
              aria-pressed={isActive}
              onClick={() => onSelect(item.id)}
              className={[
                "inline-flex h-11 shrink-0 items-center gap-2.5 rounded-[14px] border px-5 text-[13px] font-semibold transition duration-200",
                isActive
                  ? "border-transparent bg-gradient-to-l from-[#7C3AED] via-[#6D28D9] to-[#4F46E5] text-white shadow-[0_0_22px_rgba(124,58,237,0.55)]"
                  : "border-white/10 bg-[#0F1626]/90 text-[#E5E7EB] hover:border-[rgba(168,85,247,0.4)] hover:text-white",
              ].join(" ")}
            >
              <span>{item.label}</span>
              <FilterIcon
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
