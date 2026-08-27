"use client";

import {
  NEWS_CATEGORY_FILTERS,
  type NewsCategoryId,
} from "@/data/news-articles";

type NewsCategoryFilterProps = {
  active: NewsCategoryId;
  onSelect: (value: NewsCategoryId) => void;
};

/** Icon colors from reference (inactive state). */
const ICON_COLOR: Record<string, string> = {
  all: "#FFFFFF",
  ai: "#A78BFA",
  code: "#C4B5FD",
  dotnet: "#C084FC",
  frontend: "#22D3EE",
  backend: "#818CF8",
  devops: "#38BDF8",
  tools: "#C084FC",
  security: "#67E8F9",
};

function CategoryIcon({ name, color }: { name: string; color: string }) {
  const s = color;
  const cls = "h-4 w-4 shrink-0";

  switch (name) {
    case "ai":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M9.5 8.2h5M9.5 15.8h5M8.2 12h7.6"
            stroke={s}
            strokeWidth="1.6"
            strokeLinecap="round"
          />
          <path
            d="M12 3.8c1.1 0 2 .6 2.5 1.5A3.3 3.3 0 0 1 18.2 8.5c0 .5-.1 1-.3 1.4.8.7 1.3 1.7 1.3 2.8a4 4 0 0 1-4 4h-.7c-.5 1.2-1.6 2-2.9 2s-2.4-.8-2.9-2h-.7a4 4 0 0 1-4-4c0-1.1.5-2.1 1.3-2.8A3.2 3.2 0 0 1 5.5 8.5 3.3 3.3 0 0 1 9.5 5.3C10 4.4 10.9 3.8 12 3.8Z"
            stroke={s}
            strokeWidth="1.55"
            strokeLinejoin="round"
          />
        </svg>
      );
    case "code":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M9 7.2 4.2 12 9 16.8M15 7.2 19.8 12 15 16.8"
            stroke={s}
            strokeWidth="1.8"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
      );
    case "dotnet":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M12 3.8 20 8v8l-8 4.2L4 16V8l8-4.2Z"
            stroke={s}
            strokeWidth="1.55"
            strokeLinejoin="round"
          />
          <path d="M12 12v8.2M12 12 20 8M12 12 4 8" stroke={s} strokeWidth="1.45" />
        </svg>
      );
    case "frontend":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="3.2" y="4.2" width="17.6" height="12.8" rx="2" stroke={s} strokeWidth="1.55" />
          <path d="M3.2 8.4h17.6M8.5 20h7M12 17v3" stroke={s} strokeWidth="1.55" strokeLinecap="round" />
        </svg>
      );
    case "backend":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="3.8" y="3.5" width="16.4" height="5.1" rx="1.5" stroke={s} strokeWidth="1.55" />
          <rect x="3.8" y="9.45" width="16.4" height="5.1" rx="1.5" stroke={s} strokeWidth="1.55" />
          <rect x="3.8" y="15.4" width="16.4" height="5.1" rx="1.5" stroke={s} strokeWidth="1.55" />
          <circle cx="7.2" cy="6.05" r="0.85" fill={s} />
          <circle cx="7.2" cy="12" r="0.85" fill={s} />
          <circle cx="7.2" cy="17.95" r="0.85" fill={s} />
        </svg>
      );
    case "devops":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M8 8c2.1-2.7 5.9-2.7 8 0 1.6 2.1 1.6 4.9 0 7-2.1 2.7-5.9 2.7-8 0-1.6-2.1-1.6-4.9 0-7Z"
            stroke={s}
            strokeWidth="1.65"
            strokeLinejoin="round"
          />
          <path
            d="M16 8c2.1-2.7 5.9-2.7 8 0 .2.3.4.6.5 1M8 16c-2.1 2.7-5.9 2.7-8 0A5.6 5.6 0 0 1 .5 15"
            stroke={s}
            strokeWidth="1.65"
            strokeLinecap="round"
          />
        </svg>
      );
    case "tools":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M14.4 4.8a3.7 3.7 0 0 0 4.4 4.3L15 13.1l-4-4 3.4-4.3ZM9.2 14.8 4.5 19.5"
            stroke={s}
            strokeWidth="1.65"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="M8.3 9.6 4.7 6a2.4 2.4 0 0 1 3.4-3.4l3.6 3.6"
            stroke={s}
            strokeWidth="1.65"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
      );
    case "security":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M12 3 19.5 6.2v5.5c0 4.5-3.1 7.9-7.5 9.4-4.4-1.5-7.5-4.9-7.5-9.4V6.2L12 3Z"
            stroke={s}
            strokeWidth="1.55"
            strokeLinejoin="round"
          />
          <path
            d="M9.6 12.1 11.3 13.8 14.7 10.2"
            stroke={s}
            strokeWidth="1.7"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
      );
    default:
      /* 2×2 grid — "همه" */
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="3.5" y="3.5" width="7" height="7" rx="1.5" stroke={s} strokeWidth="1.6" />
          <rect x="13.5" y="3.5" width="7" height="7" rx="1.5" stroke={s} strokeWidth="1.6" />
          <rect x="3.5" y="13.5" width="7" height="7" rx="1.5" stroke={s} strokeWidth="1.6" />
          <rect x="13.5" y="13.5" width="7" height="7" rx="1.5" stroke={s} strokeWidth="1.6" />
        </svg>
      );
  }
}

/**
 * Category pills matching the reference strip:
 * purple→blue active gradient, dark bordered inactive, colored icons left of label.
 */
export function NewsCategoryFilter({ active, onSelect }: NewsCategoryFilterProps) {
  return (
    <div
      className="overflow-x-auto [-ms-overflow-style:none] [scrollbar-width:none] [&::-webkit-scrollbar]:hidden"
      dir="ltr"
    >
      <div
        className="flex w-max min-w-full flex-nowrap items-center gap-3"
        role="toolbar"
        aria-label="فیلتر دسته‌بندی اخبار"
      >
        {NEWS_CATEGORY_FILTERS.map((item) => {
          const isActive = active === item.id;
          return (
            <button
              key={item.id}
              type="button"
              aria-pressed={isActive}
              onClick={() => onSelect(item.id)}
              className={[
                "inline-flex h-[42px] shrink-0 items-center gap-2 rounded-[12px] border px-4 text-[13px] font-semibold transition duration-200",
                isActive
                  ? "border-transparent bg-gradient-to-r from-[#7C3AED] to-[#3B82F6] text-white shadow-[0_0_20px_rgba(124,58,237,0.45)]"
                  : "border-white/[0.14] bg-[#111827]/90 text-[#E5E7EB] hover:border-[rgba(168,85,247,0.4)] hover:bg-[#1E293B] hover:text-white",
              ].join(" ")}
            >
              <CategoryIcon
                name={item.icon}
                color={isActive ? "#FFFFFF" : ICON_COLOR[item.icon] ?? "#A78BFA"}
              />
              <span>{item.label}</span>
            </button>
          );
        })}
      </div>
    </div>
  );
}
