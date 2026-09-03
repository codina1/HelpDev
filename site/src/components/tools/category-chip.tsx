"use client";

import { useEffect, useRef } from "react";
import { TOOL_CATEGORY_CHIPS, type ToolCategoryId } from "@/data/tools";

type CategoryChipBarProps = {
  active: ToolCategoryId;
  onSelect: (value: ToolCategoryId) => void;
};

const ICON_COLOR: Record<string, string> = {
  all: "#FFFFFF",
  ai: "#A78BFA",
  web: "#22D3EE",
  frontend: "#60A5FA",
  backend: "#818CF8",
  devops: "#38BDF8",
  design: "#F472B6",
  security: "#F87171",
  database: "#34D399",
  mobile: "#A5B4FC",
};

function CategoryIcon({ name, color }: { name: string; color: string }) {
  const cls = "h-5 w-5 shrink-0";
  switch (name) {
    case "ai":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <circle cx="12" cy="12" r="3.2" stroke={color} strokeWidth="1.6" />
          <path d="M12 3.5v3M12 17.5v3M3.5 12h3M17.5 12h3M6.2 6.2l2.1 2.1M15.7 15.7l2.1 2.1M17.8 6.2l-2.1 2.1M8.3 15.7l-2.1 2.1" stroke={color} strokeWidth="1.6" strokeLinecap="round" />
        </svg>
      );
    case "web":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <circle cx="12" cy="12" r="8" stroke={color} strokeWidth="1.6" />
          <path d="M4.5 12h15M12 4.5c2.4 2.4 3.6 5 3.6 7.5S14.4 17.1 12 19.5c-2.4-2.4-3.6-5-3.6-7.5S9.6 6.9 12 4.5Z" stroke={color} strokeWidth="1.6" />
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
    case "design":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <circle cx="12" cy="12" r="8" stroke={color} strokeWidth="1.6" />
          <circle cx="12" cy="12" r="2.4" fill={color} />
        </svg>
      );
    case "security":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M12 3.5 5.5 6.2v5.2c0 4.2 2.8 7.5 6.5 8.8 3.7-1.3 6.5-4.6 6.5-8.8V6.2L12 3.5Z" stroke={color} strokeWidth="1.6" strokeLinejoin="round" />
        </svg>
      );
    case "database":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <ellipse cx="12" cy="6.2" rx="7" ry="2.8" stroke={color} strokeWidth="1.55" />
          <path d="M5 6.2v11.4c0 1.55 3.1 2.8 7 2.8s7-1.25 7-2.8V6.2" stroke={color} strokeWidth="1.55" />
          <path d="M5 12c0 1.55 3.1 2.8 7 2.8s7-1.25 7-2.8" stroke={color} strokeWidth="1.55" />
        </svg>
      );
    case "mobile":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="7" y="2.8" width="10" height="18.4" rx="2.4" stroke={color} strokeWidth="1.6" />
          <path d="M10.6 18.4h2.8" stroke={color} strokeWidth="1.6" strokeLinecap="round" />
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

/** Horizontal scrollable category chips under the hero. */
export function CategoryChipBar({ active, onSelect }: CategoryChipBarProps) {
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
      <div className="flex w-max min-w-full flex-nowrap items-center gap-3" role="toolbar" aria-label="فیلتر دسته‌بندی ابزارها">
        {TOOL_CATEGORY_CHIPS.map((item) => {
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
