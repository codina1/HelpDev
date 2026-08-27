"use client";

import {
  NEWS_CATEGORY_FILTERS,
  type NewsCategoryId,
} from "@/data/news-articles";

type NewsCategoryFilterProps = {
  active: NewsCategoryId;
  onSelect: (value: NewsCategoryId) => void;
};

function CategoryIcon({ name }: { name: string }) {
  const common = "h-4 w-4 shrink-0";
  switch (name) {
    case "ai":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M9.5 8.5h5M9.5 15.5h5M8 11h8"
            stroke="currentColor"
            strokeWidth="1.7"
            strokeLinecap="round"
          />
          <path
            d="M12 3.5c1.2 0 2.2.7 2.7 1.7A3.5 3.5 0 0 1 18 8.5c0 .6-.1 1.1-.4 1.6.7.7 1.1 1.7 1.1 2.8a4.2 4.2 0 0 1-4.2 4.2h-.5c-.4 1.2-1.5 2-2.8 2s-2.4-.8-2.8-2h-.5A4.2 4.2 0 0 1 4 12.9c0-1.1.4-2.1 1.1-2.8A3.4 3.4 0 0 1 4.7 8.5 3.5 3.5 0 0 1 9.3 5.2C9.8 4.2 10.8 3.5 12 3.5Z"
            stroke="currentColor"
            strokeWidth="1.7"
            strokeLinejoin="round"
          />
        </svg>
      );
    case "code":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M8.5 7.5 4 12l4.5 4.5M15.5 7.5 20 12l-4.5 4.5"
            stroke="currentColor"
            strokeWidth="1.8"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
      );
    case "dotnet":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M12 4.5 19 8v8l-7 3.5L5 16V8l7-3.5Z"
            stroke="currentColor"
            strokeWidth="1.7"
            strokeLinejoin="round"
          />
          <path d="M12 12v7.5M12 12 19 8M12 12 5 8" stroke="currentColor" strokeWidth="1.7" />
        </svg>
      );
    case "frontend":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="3.5" y="5" width="17" height="12" rx="2" stroke="currentColor" strokeWidth="1.7" />
          <path d="M8 20h8M12 17v3" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" />
        </svg>
      );
    case "backend":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="4" y="4" width="16" height="5" rx="1.5" stroke="currentColor" strokeWidth="1.7" />
          <rect x="4" y="10" width="16" height="5" rx="1.5" stroke="currentColor" strokeWidth="1.7" />
          <rect x="4" y="16" width="16" height="4" rx="1.5" stroke="currentColor" strokeWidth="1.7" />
          <circle cx="7.5" cy="6.5" r="0.8" fill="currentColor" />
          <circle cx="7.5" cy="12.5" r="0.8" fill="currentColor" />
          <circle cx="7.5" cy="18" r="0.8" fill="currentColor" />
        </svg>
      );
    case "devops":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M8.5 8.5c1.8-2.4 5.2-2.4 7 0 1.4 1.8 1.4 4.2 0 6-1.8 2.4-5.2 2.4-7 0-1.4-1.8-1.4-4.2 0-6Z"
            stroke="currentColor"
            strokeWidth="1.7"
            strokeLinejoin="round"
          />
          <path
            d="M15.5 8.5c1.8-2.4 5.2-2.4 7 0 .3.4.5.8.6 1.2M8.5 15.5c-1.8 2.4-5.2 2.4-7 0A5 5 0 0 1 1 14.3"
            stroke="currentColor"
            strokeWidth="1.7"
            strokeLinecap="round"
          />
        </svg>
      );
    case "tools":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M14.5 5.5a3.5 3.5 0 0 0 4 4L15 13l-4-4 3.5-3.5ZM9 15l-4.5 4.5"
            stroke="currentColor"
            strokeWidth="1.7"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="M8 9.5 4.8 6.3a2.2 2.2 0 0 1 3.1-3.1L11 6.3"
            stroke="currentColor"
            strokeWidth="1.7"
            strokeLinecap="round"
          />
        </svg>
      );
    case "security":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M12 3.5 19 6.5v5.2c0 4.2-2.8 7.4-7 8.8-4.2-1.4-7-4.6-7-8.8V6.5l7-3Z"
            stroke="currentColor"
            strokeWidth="1.7"
            strokeLinejoin="round"
          />
          <path d="M12 11v3.5" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" />
          <circle cx="12" cy="9.2" r="1" fill="currentColor" />
        </svg>
      );
    default:
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="3.5" y="3.5" width="7" height="7" rx="1.5" stroke="currentColor" strokeWidth="1.7" />
          <rect x="13.5" y="3.5" width="7" height="7" rx="1.5" stroke="currentColor" strokeWidth="1.7" />
          <rect x="3.5" y="13.5" width="7" height="7" rx="1.5" stroke="currentColor" strokeWidth="1.7" />
          <rect x="13.5" y="13.5" width="7" height="7" rx="1.5" stroke="currentColor" strokeWidth="1.7" />
        </svg>
      );
  }
}

/** Horizontal category pills under the news hero — matches reference. */
export function NewsCategoryFilter({ active, onSelect }: NewsCategoryFilterProps) {
  return (
    <div
      className="overflow-x-auto pb-1 [-ms-overflow-style:none] [scrollbar-width:none] [&::-webkit-scrollbar]:hidden"
      dir="rtl"
    >
      <div className="flex w-max min-w-full flex-nowrap justify-start gap-2.5 sm:gap-3" role="toolbar" aria-label="فیلتر دسته‌بندی اخبار">
        {NEWS_CATEGORY_FILTERS.map((item) => {
          const isActive = active === item.id;
          return (
            <button
              key={item.id}
              type="button"
              aria-pressed={isActive}
              onClick={() => onSelect(item.id)}
              className={[
                "inline-flex shrink-0 items-center gap-2 rounded-xl border px-3.5 py-2.5 text-[12px] font-bold transition duration-300 sm:px-4 sm:text-[13px]",
                isActive
                  ? "border-transparent bg-gradient-to-l from-[#7C3AED] to-[#6D28D9] text-white shadow-[0_0_22px_rgba(124,58,237,0.45)]"
                  : "border-white/[0.1] bg-[#0F172A]/90 text-[#CBD5E1] hover:border-[rgba(168,85,247,0.4)] hover:bg-[rgba(124,58,237,0.12)] hover:text-white",
              ].join(" ")}
            >
              <span className={isActive ? "text-white" : "text-[#A78BFA]"}>
                <CategoryIcon name={item.icon} />
              </span>
              {item.label}
            </button>
          );
        })}
      </div>
    </div>
  );
}
